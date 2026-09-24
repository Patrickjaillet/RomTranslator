// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Localization;
using RomTranslator.Core.Projects;
using RomTranslator.Modules.SegaSaturn.Disc;

namespace RomTranslator.Modules.SegaSaturn.TextExtraction;

/// <summary>
/// Extrait les blocs de texte d'une image disque Sega Saturn (BIN/CUE), par balayage heuristique de toute
/// la piste de données ou sur une plage d'adresses choisie manuellement. Les occurrences de texte source
/// identiques sont regroupées sous une seule entrée (voir <see cref="ITranslationEntry.OccurrenceCount" />),
/// pour éviter de traduire plusieurs fois le même texte.
/// </summary>
public sealed class SaturnTextExtractor : ITextExtractor
{
    private readonly TextScanOptions _scanOptions;

    /// <summary>Initialise l'extracteur avec les paramètres de balayage heuristique par défaut.</summary>
    public SaturnTextExtractor()
        : this(TextScanOptions.Default)
    {
    }

    /// <summary>Initialise l'extracteur avec des paramètres de balayage heuristique explicites.</summary>
    public SaturnTextExtractor(TextScanOptions scanOptions)
    {
        ArgumentNullException.ThrowIfNull(scanOptions);
        scanOptions.Validate();

        _scanOptions = scanOptions;
    }

    /// <inheritdoc />
    public IReadOnlyList<ITranslationEntry> ExtractAutomatically(string romPath, ICharacterTable characterTable)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);
        ArgumentNullException.ThrowIfNull(characterTable);

        CueSheet cueSheet = CueSheetReader.Read(romPath);
        CueTrack dataTrack = cueSheet.FirstDataTrack
            ?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings.Saturn_Error_NoDataTrack, romPath));

        using SectorReader sectorReader = new(dataTrack);
        long totalSectors = new FileInfo(dataTrack.DataFilePath).Length / SectorReaderStride(dataTrack);
        byte[] bytes = sectorReader.ReadBytes(0, (int)(totalSectors * SectorReader.SectorDataSize));

        List<CandidateString> candidates = ScanForCandidates(bytes, characterTable);
        return GroupIntoEntries(candidates);
    }

    /// <inheritdoc />
    public IReadOnlyList<ITranslationEntry> ExtractRange(string romPath, ICharacterTable characterTable, long startOffset, long endOffset)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);
        ArgumentNullException.ThrowIfNull(characterTable);
        if (endOffset < startOffset)
        {
            throw new ArgumentOutOfRangeException(nameof(endOffset), endOffset, "Le décalage de fin doit être supérieur ou égal au décalage de début.");
        }

        CueSheet cueSheet = CueSheetReader.Read(romPath);
        CueTrack dataTrack = cueSheet.FirstDataTrack
            ?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings.Saturn_Error_NoDataTrack, romPath));

        using SectorReader sectorReader = new(dataTrack);
        long startSector = startOffset / SectorReader.SectorDataSize;
        int sectorAlignedStart = (int)(startOffset - (startSector * SectorReader.SectorDataSize));
        int length = (int)(endOffset - startOffset);

        byte[] sectorAlignedBytes = sectorReader.ReadBytes(startSector, sectorAlignedStart + length);
        byte[] rangeBytes = new byte[length];
        Array.Copy(sectorAlignedBytes, sectorAlignedStart, rangeBytes, 0, length);

        // Une extraction manuelle décode la plage entière comme une seule chaîne : c'est le rôle de
        // l'utilisateur d'avoir choisi une plage qui correspond exactement à une chaîne de texte.
        string text = characterTable.Decode(rangeBytes);
        TranslationEntry entry = new(id: startOffset.ToString("X", CultureInfo.InvariantCulture), sourceText: text, offset: startOffset);

        return new ITranslationEntry[] { entry };
    }

    private List<CandidateString> ScanForCandidates(byte[] bytes, ICharacterTable characterTable)
    {
        List<CandidateString> candidates = new();
        int position = 0;

        while (position < bytes.Length)
        {
            (string? text, int length) = TryDecodeCandidateAt(bytes, position, characterTable);

            if (text is not null && text.Length >= _scanOptions.MinimumLength
                && CandidateStringScorer.Score(text) >= _scanOptions.MinimumScore)
            {
                candidates.Add(new CandidateString(position, text));
                position += length;
            }
            else
            {
                position++;
            }
        }

        return candidates;
    }

    /// <summary>
    /// Longueur maximale, en octets, examinée pour une seule chaîne candidate. Une chaîne de dialogue de
    /// jeu dépasse rarement quelques centaines de caractères ; cette limite évite un coût quadratique
    /// incontrôlé si une longue plage de la ROM se décode par coïncidence.
    /// </summary>
    private const int MaxCandidateByteLength = 512;

    /// <summary>
    /// Détermine la plus longue chaîne décodable en partant de <paramref name="start" />. Le succès de
    /// <see cref="ICharacterTable.Decode" /> sur une longueur donnée n'est pas monotone (une table DTE/MTE
    /// peut définir une séquence de 2 octets sans qu'aucun des deux octets pris individuellement ne soit
    /// valide), donc chaque longueur est testée indépendamment plutôt que par recherche dichotomique ; les
    /// chaînes de texte de jeu restent courtes, ce qui garde ce parcours linéaire largement suffisant.
    /// </summary>
    private static (string? Text, int ByteLength) TryDecodeCandidateAt(byte[] bytes, int start, ICharacterTable characterTable)
    {
        int maxAvailable = Math.Min(bytes.Length - start, MaxCandidateByteLength);
        int longestDecodable = 0;
        string? longestText = null;

        for (int length = 1; length <= maxAvailable; length++)
        {
            byte[] slice = new byte[length];
            Array.Copy(bytes, start, slice, 0, length);

            try
            {
                longestText = characterTable.Decode(slice);
                longestDecodable = length;
            }
            catch (ArgumentException)
            {
                // Cette longueur ne se décode pas entièrement ; une longueur plus grande peut néanmoins
                // réussir si la table définit une séquence multi-octets qui englobe ce point (DTE/MTE).
            }
        }

        return longestDecodable == 0 ? (null, 0) : (longestText, longestDecodable);
    }

    private static List<ITranslationEntry> GroupIntoEntries(List<CandidateString> candidates)
    {
        // Un même texte source peut apparaître à plusieurs offsets ; la première occurrence rencontrée
        // fixe l'offset et l'identifiant de l'entrée regroupée, les suivantes n'incrémentent que le compteur.
        Dictionary<string, (long FirstOffset, int Count)> occurrencesByText = new(StringComparer.Ordinal);
        List<string> textsInFirstSeenOrder = new();

        foreach (CandidateString candidate in candidates)
        {
            if (occurrencesByText.TryGetValue(candidate.Text, out (long FirstOffset, int Count) existing))
            {
                occurrencesByText[candidate.Text] = (existing.FirstOffset, existing.Count + 1);
            }
            else
            {
                occurrencesByText[candidate.Text] = (candidate.Offset, 1);
                textsInFirstSeenOrder.Add(candidate.Text);
            }
        }

        List<ITranslationEntry> entries = new(textsInFirstSeenOrder.Count);
        foreach (string text in textsInFirstSeenOrder)
        {
            (long offset, int count) = occurrencesByText[text];
            entries.Add(new TranslationEntry(
                id: offset.ToString("X", CultureInfo.InvariantCulture),
                sourceText: text,
                offset: offset,
                occurrenceCount: count));
        }

        return entries;
    }

    private static int SectorReaderStride(CueTrack track)
    {
        return track.Mode switch
        {
            CueTrackMode.Mode1Cooked => SectorReader.SectorDataSize,
            CueTrackMode.Mode1Raw or CueTrackMode.Mode2 => 2352,
            _ => SectorReader.SectorDataSize,
        };
    }

    private sealed record CandidateString(long Offset, string Text);
}
