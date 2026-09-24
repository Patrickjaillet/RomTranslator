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
    /// jeu dépasse rarement quelques centaines de caractères ; cette limite plafonne le nombre d'itérations
    /// de la recherche par doublement dans <see cref="TryDecodeCandidateAt" /> (voir sa documentation).
    /// </summary>
    private const int MaxCandidateByteLength = 512;

    /// <summary>
    /// Détermine la plus longue chaîne décodable en partant de <paramref name="start" />, en au plus
    /// <c>O(log MaxCandidateByteLength)</c> appels à <see cref="ICharacterTable.Decode" /> dans le cas courant.
    /// Une recherche linéaire (essayer chaque longueur de 1 à <see cref="MaxCandidateByteLength" />) coûterait
    /// un décodage complet par longueur testée, soit un coût quadratique par position de balayage — bien trop
    /// lent dès qu'une plage de plusieurs centaines d'octets se décode entièrement (par exemple une zone de
    /// remplissage constituée d'espaces). La recherche procède donc par doublement (1, 2, 4, 8...) jusqu'à
    /// dépasser la longueur maximale décodable, puis affine par dichotomie entre le dernier échec et le
    /// dernier succès. Le succès de <see cref="ICharacterTable.Decode" /> sur une longueur donnée n'est pas
    /// garanti strictement monotone (une table DTE/MTE peut définir une séquence multi-octets sans qu'aucun
    /// octet pris individuellement ne soit valide), donc cette recherche peut dans de rares cas ne pas trouver
    /// la longueur décodable la plus longue possible ; il s'agit d'un compromis délibéré pour un balayage
    /// heuristique (voir <c>MEMOIRE.md</c>), pas d'une désérialisation qui exigerait une exactitude totale.
    /// </summary>
    private static (string? Text, int ByteLength) TryDecodeCandidateAt(byte[] bytes, int start, ICharacterTable characterTable)
    {
        int maxAvailable = Math.Min(bytes.Length - start, MaxCandidateByteLength);

        int lastSuccess = 0;
        int lastFailure = 0;
        int length = 1;

        while (length <= maxAvailable)
        {
            if (TryDecode(bytes, start, length, characterTable, out _))
            {
                lastSuccess = length;
                length *= 2;
            }
            else
            {
                lastFailure = length;
                break;
            }
        }

        if (lastFailure > 0)
        {
            int low = lastSuccess;
            int high = lastFailure;

            while (high - low > 1)
            {
                int mid = low + ((high - low) / 2);
                if (TryDecode(bytes, start, mid, characterTable, out _))
                {
                    low = mid;
                }
                else
                {
                    high = mid;
                }
            }

            lastSuccess = low;
        }

        if (lastSuccess == 0)
        {
            return (null, 0);
        }

        TryDecode(bytes, start, lastSuccess, characterTable, out string? text);
        return (text, lastSuccess);
    }

    private static bool TryDecode(byte[] bytes, int start, int length, ICharacterTable characterTable, out string? text)
    {
        byte[] slice = new byte[length];
        Array.Copy(bytes, start, slice, 0, length);

        try
        {
            text = characterTable.Decode(slice);
            return true;
        }
        catch (ArgumentException)
        {
            text = null;
            return false;
        }
    }

    private static List<ITranslationEntry> GroupIntoEntries(List<CandidateString> candidates)
    {
        // Un même texte source peut apparaître à plusieurs offsets ; toutes les occurrences sont conservées
        // (nécessaire à la réinjection, Phase 5.6), dans l'ordre où elles ont été rencontrées.
        Dictionary<string, List<long>> offsetsByText = new(StringComparer.Ordinal);
        List<string> textsInFirstSeenOrder = new();

        foreach (CandidateString candidate in candidates)
        {
            if (offsetsByText.TryGetValue(candidate.Text, out List<long>? offsets))
            {
                offsets.Add(candidate.Offset);
            }
            else
            {
                offsetsByText[candidate.Text] = new List<long> { candidate.Offset };
                textsInFirstSeenOrder.Add(candidate.Text);
            }
        }

        List<ITranslationEntry> entries = new(textsInFirstSeenOrder.Count);
        foreach (string text in textsInFirstSeenOrder)
        {
            List<long> offsets = offsetsByText[text];
            entries.Add(new TranslationEntry(
                id: offsets[0].ToString("X", CultureInfo.InvariantCulture),
                sourceText: text,
                offsets: offsets));
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
