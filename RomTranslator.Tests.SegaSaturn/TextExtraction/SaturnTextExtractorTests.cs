// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Text;
using RomTranslator.Core.Abstractions;
using RomTranslator.Modules.SegaSaturn.CharacterTables;
using RomTranslator.Modules.SegaSaturn.TextExtraction;
using RomTranslator.Tests.SegaSaturn.Support;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.TextExtraction;

public sealed class SaturnTextExtractorTests
{
    private static SaturnCharacterTable CreateAsciiTable()
    {
        CharacterTableFile file = CharacterTableFileReader.Read(FindAsciiTablePath());
        return new SaturnCharacterTable("ASCII", file);
    }

    private static string FindAsciiTablePath()
    {
        return System.IO.Path.Combine(AppContext.BaseDirectory, "PredefinedTables", "ascii.tbl");
    }

    [Fact]
    public void ExtractAutomatically_finds_an_embedded_ascii_string()
    {
        using TemporaryDirectory temp = new();
        byte[] payload = Encoding.ASCII.GetBytes("Hello, adventurer!");
        // Décalage au sein d'un secteur de remplissage (secteur 5, dans la zone système synthétique).
        long offset = 5L * 2048;
        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath, payloadBytes: payload, payloadOffset: offset);

        SaturnTextExtractor extractor = new();
        var entries = extractor.ExtractAutomatically(cuePath, CreateAsciiTable());

        Assert.Contains(entries, entry => entry.SourceText == "Hello, adventurer!");
    }

    [Fact]
    public void ExtractAutomatically_groups_identical_occurrences()
    {
        using TemporaryDirectory temp = new();
        byte[] first = Encoding.ASCII.GetBytes("Attack!");
        byte[] second = Encoding.ASCII.GetBytes("Attack!");
        byte[] payload = new byte[first.Length + 4 + second.Length];
        Array.Copy(first, 0, payload, 0, first.Length);
        Array.Copy(second, 0, payload, first.Length + 4, second.Length);

        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath, payloadBytes: payload, payloadOffset: 5L * 2048);

        SaturnTextExtractor extractor = new();
        var entries = extractor.ExtractAutomatically(cuePath, CreateAsciiTable());

        ITranslationEntry entry = Assert.Single(entries, e => e.SourceText == "Attack!");
        Assert.Equal(2, entry.OccurrenceCount);
    }

    [Fact]
    public void ExtractAutomatically_ignores_candidates_shorter_than_the_minimum_length()
    {
        using TemporaryDirectory temp = new();
        byte[] payload = Encoding.ASCII.GetBytes("Hi");
        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath, payloadBytes: payload, payloadOffset: 5L * 2048);

        SaturnTextExtractor extractor = new(new TextScanOptions(MinimumLength: 4, MinimumScore: 0));
        var entries = extractor.ExtractAutomatically(cuePath, CreateAsciiTable());

        Assert.DoesNotContain(entries, entry => entry.SourceText == "Hi");
    }

    [Fact]
    public void ExtractAutomatically_finds_a_short_string_when_the_minimum_length_is_lowered()
    {
        using TemporaryDirectory temp = new();
        byte[] payload = Encoding.ASCII.GetBytes("Hi");
        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath, payloadBytes: payload, payloadOffset: 5L * 2048);

        SaturnTextExtractor extractor = new(new TextScanOptions(MinimumLength: 2, MinimumScore: 0));
        var entries = extractor.ExtractAutomatically(cuePath, CreateAsciiTable());

        Assert.Contains(entries, entry => entry.SourceText == "Hi");
    }

    [Fact]
    public void ExtractRange_decodes_exactly_the_requested_bytes()
    {
        using TemporaryDirectory temp = new();
        byte[] payload = Encoding.ASCII.GetBytes("Exact range");
        long offset = 5L * 2048;
        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath, payloadBytes: payload, payloadOffset: offset);

        SaturnTextExtractor extractor = new();
        var entries = extractor.ExtractRange(cuePath, CreateAsciiTable(), offset, offset + payload.Length);

        ITranslationEntry entry = Assert.Single(entries);
        Assert.Equal("Exact range", entry.SourceText);
        Assert.Equal(offset, entry.Offset);
    }

    [Fact]
    public void ExtractRange_rejects_an_end_offset_before_the_start_offset()
    {
        using TemporaryDirectory temp = new();
        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath);

        SaturnTextExtractor extractor = new();

        Assert.Throws<ArgumentOutOfRangeException>(() => extractor.ExtractRange(cuePath, CreateAsciiTable(), 100, 50));
    }

    [Fact]
    public void Constructor_rejects_invalid_scan_options()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SaturnTextExtractor(new TextScanOptions(0, 0.5)));
    }
}
