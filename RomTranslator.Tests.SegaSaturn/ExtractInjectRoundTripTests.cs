// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;
using System.Linq;
using System.Text;
using RomTranslator.Core.Abstractions;
using RomTranslator.Modules.SegaSaturn.CharacterTables;
using RomTranslator.Modules.SegaSaturn.TextExtraction;
using RomTranslator.Modules.SegaSaturn.Translation;
using RomTranslator.Tests.SegaSaturn.Support;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn;

/// <summary>
/// Vérifie le cycle complet extraction → traduction → réinjection → nouvelle extraction sur une image disque
/// entièrement synthétique : la chaîne relue dans la ROM générée doit être exactement la traduction saisie,
/// pas le texte source d'origine ni une version tronquée ou décalée.
/// </summary>
public sealed class ExtractInjectRoundTripTests
{
    private static SaturnCharacterTable CreateAsciiTable()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "PredefinedTables", "ascii.tbl");
        CharacterTableFile file = CharacterTableFileReader.Read(path);
        return new SaturnCharacterTable("ASCII", file);
    }

    [Fact]
    public void Round_trip_preserves_a_translation_that_fits_the_source_length()
    {
        using TemporaryDirectory temp = new();
        byte[] payload = Encoding.ASCII.GetBytes("Hello, adventurer!");
        string sourceCue = SyntheticSaturnDiscBuilder.Build(temp.FullPath, payloadBytes: payload, payloadOffset: 5L * 2048);
        string outputCue = Path.Combine(temp.FullPath, "output", "test.cue");

        SaturnCharacterTable table = CreateAsciiTable();
        SaturnTextExtractor extractor = new();
        SaturnTextInjector injector = new();

        var extracted = extractor.ExtractAutomatically(sourceCue, table);
        ITranslationEntry entry = extracted.Single(e => e.SourceText == "Hello, adventurer!");
        entry.TranslatedText = "Salut, voyageur!";

        injector.Inject(sourceCue, outputCue, extracted, table);

        var reExtracted = extractor.ExtractAutomatically(outputCue, table);
        Assert.Contains(reExtracted, e => e.SourceText.TrimEnd() == "Salut, voyageur!");
        Assert.DoesNotContain(reExtracted, e => e.SourceText == "Hello, adventurer!");
    }

    [Fact]
    public void Round_trip_preserves_every_occurrence_of_a_duplicated_string()
    {
        using TemporaryDirectory temp = new();
        byte[] payload = new byte[20];
        Array.Copy(Encoding.ASCII.GetBytes("Attack"), 0, payload, 0, 6);
        Array.Copy(Encoding.ASCII.GetBytes("Attack"), 0, payload, 10, 6);
        string sourceCue = SyntheticSaturnDiscBuilder.Build(temp.FullPath, payloadBytes: payload, payloadOffset: 5L * 2048);
        string outputCue = Path.Combine(temp.FullPath, "output", "test.cue");

        SaturnCharacterTable table = CreateAsciiTable();
        SaturnTextExtractor extractor = new();
        SaturnTextInjector injector = new();

        var extracted = extractor.ExtractAutomatically(sourceCue, table);
        ITranslationEntry entry = extracted.Single(e => e.SourceText == "Attack");
        Assert.Equal(2, entry.OccurrenceCount);
        entry.TranslatedText = "Frappe";

        injector.Inject(sourceCue, outputCue, extracted, table);

        var reExtracted = extractor.ExtractAutomatically(outputCue, table);
        ITranslationEntry reExtractedEntry = reExtracted.Single(e => e.SourceText == "Frappe");
        Assert.Equal(2, reExtractedEntry.OccurrenceCount);
    }

    [Fact]
    public void Round_trip_relocates_a_longer_translation_and_it_is_recovered_at_its_new_location()
    {
        using TemporaryDirectory temp = new();
        long textOffset = 5L * 2048;
        long pointerOffset = 10L * 2048;
        long freeSpaceOffset = 12L * 2048;

        byte[] pointerBytes = new byte[4];
        WritePointer(pointerBytes, textOffset);
        byte[] payload = new byte[pointerOffset + pointerBytes.Length - textOffset];
        Array.Copy(Encoding.ASCII.GetBytes("Hi"), 0, payload, 0, 2);
        Array.Copy(pointerBytes, 0, payload, pointerOffset - textOffset, pointerBytes.Length);

        string sourceCue = SyntheticSaturnDiscBuilder.Build(temp.FullPath, payloadBytes: payload, payloadOffset: textOffset);
        string outputCue = Path.Combine(temp.FullPath, "output", "test.cue");

        SaturnCharacterTable table = CreateAsciiTable();
        SaturnTextExtractor extractor = new(new TextScanOptions(MinimumLength: 2, MinimumScore: 0));
        SaturnTextInjector injector = new();

        var extracted = extractor.ExtractAutomatically(sourceCue, table);
        ITranslationEntry entry = extracted.Single(e => e.SourceText == "Hi");
        entry.TranslatedText = "Hello there";

        PointerEncoding pointerEncoding = new(ByteWidth: 4, IsBigEndian: true, IsRelative: false, BaseOffset: 0);
        SaturnPointerRelocation relocation = new(
            pointerEncoding,
            new System.Collections.Generic.Dictionary<long, System.Collections.Generic.IReadOnlyList<long>> { [textOffset] = new long[] { pointerOffset } },
            freeSpaceOffset,
            FreeSpaceLength: 64);

        injector.Inject(sourceCue, outputCue, extracted, table, relocation);

        var reExtracted = extractor.ExtractAutomatically(outputCue, table);
        Assert.Contains(reExtracted, e => e.SourceText.Contains("Hello there", StringComparison.Ordinal) && e.Offset == freeSpaceOffset);
    }

    private static void WritePointer(byte[] buffer, long value)
    {
        buffer[0] = (byte)((value >> 24) & 0xFF);
        buffer[1] = (byte)((value >> 16) & 0xFF);
        buffer[2] = (byte)((value >> 8) & 0xFF);
        buffer[3] = (byte)(value & 0xFF);
    }
}
