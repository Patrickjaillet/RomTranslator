// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Localization;
using RomTranslator.Modules.SegaSaturn.Disc;
using RomTranslator.Modules.SegaSaturn.TextExtraction;

namespace RomTranslator.Modules.SegaSaturn.Translation;

/// <summary>
/// Réinjecte les traductions dans une copie de l'image disque Sega Saturn (BIN/CUE) : réécrit chaque
/// occurrence du texte source à son emplacement d'origine si la traduction encodée tient dans le même
/// nombre d'octets (complétée par des espaces sinon), ou la reloge dans une zone de réserve désignée et met à
/// jour les pointeurs connus si <see cref="SaturnPointerRelocation" /> est fourni. Ne modifie jamais l'image
/// source : une copie complète (<c>.cue</c> + fichier(s) de données) est d'abord créée à l'emplacement de
/// destination.
/// </summary>
public sealed class SaturnTextInjector : ITextInjector
{
    /// <inheritdoc />
    /// <remarks>Équivalent à <see cref="Inject(string, string, IReadOnlyList{ITranslationEntry}, ICharacterTable, SaturnPointerRelocation?)" /> sans relogement.</remarks>
    public TextInjectionResult Inject(
        string sourceRomPath,
        string outputRomPath,
        IReadOnlyList<ITranslationEntry> entries,
        ICharacterTable characterTable)
    {
        return Inject(sourceRomPath, outputRomPath, entries, characterTable, relocation: null);
    }

    /// <summary>
    /// Réinjecte les traductions, avec relogement optionnel des chaînes trop longues pour tenir à la place
    /// de leur texte source.
    /// </summary>
    /// <param name="sourceRomPath">Chemin du fichier <c>.cue</c> de l'image ROM d'origine (jamais modifiée).</param>
    /// <param name="outputRomPath">Chemin du fichier <c>.cue</c> de l'image ROM à générer.</param>
    /// <param name="entries">Entrées de traduction à réinjecter.</param>
    /// <param name="characterTable">Table de caractères à utiliser pour encoder le texte traduit.</param>
    /// <param name="relocation">
    /// Description de la zone de réserve et des pointeurs du jeu ciblé, ou <see langword="null" /> pour
    /// interdire tout relogement (une traduction trop longue fait alors échouer l'injection pour son entrée).
    /// </param>
    public TextInjectionResult Inject(
        string sourceRomPath,
        string outputRomPath,
        IReadOnlyList<ITranslationEntry> entries,
        ICharacterTable characterTable,
        SaturnPointerRelocation? relocation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceRomPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputRomPath);
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(characterTable);

        CueSheet sourceCueSheet = CueSheetReader.Read(sourceRomPath);
        CueTrack sourceDataTrack = sourceCueSheet.FirstDataTrack
            ?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings.Saturn_Error_NoDataTrack, sourceRomPath));

        CueSheet outputCueSheet = CopyImage(sourceRomPath, outputRomPath, sourceCueSheet);
        CueTrack outputDataTrack = outputCueSheet.FirstDataTrack!;

        List<string> messages = new();
        long freeSpaceCursor = relocation?.FreeSpaceOffset ?? 0;
        List<(ITranslationEntry Entry, long WrittenOffset, int WrittenLength)> writes = new();

        using (SectorWriter writer = new(outputDataTrack))
        {
            foreach (ITranslationEntry entry in entries)
            {
                if (string.IsNullOrEmpty(entry.TranslatedText))
                {
                    continue;
                }

                IReadOnlyList<byte> encoded = characterTable.Encode(entry.TranslatedText);
                long sourceLength = MeasureSourceLength(entry, characterTable);

                if (encoded.Count <= sourceLength)
                {
                    byte[] padded = PadToLength(encoded, sourceLength, characterTable);
                    foreach (long offset in entry.Offsets)
                    {
                        writer.WriteBytes(offset, padded);
                        writes.Add((entry, offset, encoded.Count));
                    }

                    continue;
                }

                if (relocation is null)
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.CurrentCulture, Strings.Saturn_Injection_TranslationTooLongNoRelocation, entry.SourceText, entry.TranslatedText));
                }

                long relocatedOffset = AllocateFreeSpace(relocation, ref freeSpaceCursor, encoded.Count);
                writer.WriteBytes(relocatedOffset, encoded);
                writes.Add((entry, relocatedOffset, encoded.Count));

                bool pointerUpdated = false;
                foreach (long sourceOffset in entry.Offsets)
                {
                    foreach (long pointerOffset in relocation.FindPointerOffsets(sourceOffset))
                    {
                        IReadOnlyList<byte> pointerBytes = new SaturnPointerTable(relocation.PointerEncoding).EncodePointer(relocatedOffset);
                        writer.WriteBytes(pointerOffset, pointerBytes);
                        pointerUpdated = true;
                    }
                }

                if (!pointerUpdated)
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.CurrentCulture, Strings.Saturn_Injection_NoKnownPointer, entry.SourceText));
                }

                messages.Add(string.Format(
                    CultureInfo.CurrentCulture, Strings.Saturn_Injection_StringRelocated, entry.SourceText, relocatedOffset));
            }
        }

        messages.AddRange(VerifyConsistency(outputDataTrack, characterTable, writes));

        return new TextInjectionResult(outputRomPath, messages);
    }

    /// <summary>
    /// Relit chaque chaîne réellement écrite dans l'image générée et vérifie qu'elle décode exactement le
    /// texte traduit attendu, pour détecter tout défaut de réinjection (mauvais décalage, table de caractères
    /// incohérente entre l'écriture et la relecture...) avant que le fichier ne soit distribué.
    /// </summary>
    private static IReadOnlyList<string> VerifyConsistency(
        CueTrack outputDataTrack, ICharacterTable characterTable, IReadOnlyList<(ITranslationEntry Entry, long WrittenOffset, int WrittenLength)> writes)
    {
        List<string> problems = new();
        using SectorReader reader = new(outputDataTrack);

        foreach ((ITranslationEntry entry, long writtenOffset, int writtenLength) in writes)
        {
            byte[] rereadBytes = reader.ReadBytes(writtenOffset / SectorReader.SectorDataSize, GetSectorAlignedLength(writtenOffset, writtenLength));
            int offsetInSector = (int)(writtenOffset - ((writtenOffset / SectorReader.SectorDataSize) * SectorReader.SectorDataSize));
            byte[] writtenBytes = new byte[writtenLength];
            Array.Copy(rereadBytes, offsetInSector, writtenBytes, 0, writtenLength);

            string decoded = characterTable.Decode(writtenBytes);
            string expected = entry.TranslatedText.PadRight(decoded.Length);
            if (!string.Equals(decoded, expected, StringComparison.Ordinal))
            {
                problems.Add(string.Format(
                    CultureInfo.CurrentCulture, Strings.Saturn_Injection_ConsistencyMismatch, entry.SourceText, writtenOffset, decoded));
            }
        }

        return problems;
    }

    private static int GetSectorAlignedLength(long offset, int length)
    {
        long sectorIndex = offset / SectorReader.SectorDataSize;
        int offsetInSector = (int)(offset - (sectorIndex * SectorReader.SectorDataSize));
        return offsetInSector + length;
    }

    private static long MeasureSourceLength(ITranslationEntry entry, ICharacterTable characterTable)
    {
        return characterTable.Encode(entry.SourceText).Count;
    }

    private static byte[] PadToLength(IReadOnlyList<byte> encoded, long targetLength, ICharacterTable characterTable)
    {
        byte[] result = new byte[targetLength];
        for (int i = 0; i < encoded.Count; i++)
        {
            result[i] = encoded[i];
        }

        if (encoded.Count < targetLength)
        {
            IReadOnlyList<byte> spaceBytes = characterTable.Encode(" ");
            if (spaceBytes.Count != 1)
            {
                throw new InvalidOperationException(Strings.Saturn_Injection_NoSpaceCharacter);
            }

            for (int i = encoded.Count; i < targetLength; i++)
            {
                result[i] = spaceBytes[0];
            }
        }

        return result;
    }

    private static long AllocateFreeSpace(SaturnPointerRelocation relocation, ref long cursor, int requiredLength)
    {
        long remaining = relocation.FreeSpaceOffset + relocation.FreeSpaceLength - cursor;
        if (requiredLength > remaining)
        {
            throw new InvalidOperationException(string.Format(
                CultureInfo.CurrentCulture, Strings.Saturn_Injection_FreeSpaceExhausted, requiredLength, remaining));
        }

        long allocatedOffset = cursor;
        cursor += requiredLength;
        return allocatedOffset;
    }

    private static CueSheet CopyImage(string sourceCuePath, string outputCuePath, CueSheet sourceCueSheet)
    {
        string outputDirectory = Path.GetDirectoryName(Path.GetFullPath(outputCuePath)) ?? string.Empty;
        Directory.CreateDirectory(outputDirectory);

        Dictionary<string, string> copiedDataFiles = new(StringComparer.OrdinalIgnoreCase);
        string cueContent = File.ReadAllText(sourceCuePath);

        foreach (CueTrack track in sourceCueSheet.Tracks)
        {
            if (copiedDataFiles.ContainsKey(track.DataFilePath))
            {
                continue;
            }

            string destinationFileName = Path.GetFileName(track.DataFilePath);
            string destinationPath = Path.Combine(outputDirectory, destinationFileName);
            File.Copy(track.DataFilePath, destinationPath, overwrite: true);
            copiedDataFiles[track.DataFilePath] = destinationPath;
        }

        File.WriteAllText(outputCuePath, cueContent);

        return CueSheetReader.Read(outputCuePath);
    }
}
