// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using RomTranslator.Modules.SegaSaturn.CharacterTables;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.CharacterTables;

public sealed class SaturnCharacterTableTests
{
    private static SaturnCharacterTable CreateAsciiTable()
    {
        CharacterTableFile file = CharacterTableFileReader.Parse(new[] { "41=A", "42=B", "43=C" });
        return new SaturnCharacterTable("Test", file);
    }

    [Fact]
    public void Decode_maps_single_byte_entries()
    {
        SaturnCharacterTable table = CreateAsciiTable();

        Assert.Equal("ABC", table.Decode(new byte[] { 0x41, 0x42, 0x43 }));
    }

    [Fact]
    public void TableFile_exposes_the_loaded_content()
    {
        CharacterTableFile file = CharacterTableFileReader.Parse(new[] { "41=A", "42=B" });
        SaturnCharacterTable table = new("Test", file);

        Assert.Same(file, table.TableFile);
        Assert.Equal(2, table.TableFile.Entries.Count);
    }

    [Fact]
    public void Encode_maps_characters_back_to_their_bytes()
    {
        SaturnCharacterTable table = CreateAsciiTable();

        Assert.Equal(new byte[] { 0x41, 0x42 }, table.Encode("AB"));
    }

    [Fact]
    public void Decode_prefers_the_longest_matching_sequence_for_DTE_MTE()
    {
        CharacterTableFile file = CharacterTableFileReader.Parse(new[] { "41=A", "4142=THE" });
        SaturnCharacterTable table = new("Test", file);

        Assert.Equal("THE", table.Decode(new byte[] { 0x41, 0x42 }));
    }

    [Fact]
    public void Encode_prefers_the_longest_matching_text_for_DTE_MTE()
    {
        CharacterTableFile file = CharacterTableFileReader.Parse(new[] { "41=T", "42=H", "4142=TH" });
        SaturnCharacterTable table = new("Test", file);

        Assert.Equal(new byte[] { 0x41, 0x42 }, table.Encode("TH"));
    }

    [Fact]
    public void Decode_round_trips_with_Encode()
    {
        SaturnCharacterTable table = CreateAsciiTable();

        IReadOnlyList<byte> encoded = table.Encode("CAB");
        Assert.Equal("CAB", table.Decode(encoded));
    }

    [Fact]
    public void Decode_throws_when_a_byte_has_no_match()
    {
        SaturnCharacterTable table = CreateAsciiTable();

        Assert.Throws<ArgumentException>(() => table.Decode(new byte[] { 0xFF }));
    }

    [Fact]
    public void Encode_throws_when_a_character_has_no_match()
    {
        SaturnCharacterTable table = CreateAsciiTable();

        Assert.Throws<ArgumentException>(() => table.Encode("Z"));
    }

    [Fact]
    public void Validate_reports_a_byte_sequence_mapped_to_two_different_texts()
    {
        CharacterTableFile file = new(
            new List<CharacterTableEntry> { new(new byte[] { 0x41 }, "A"), new(new byte[] { 0x41 }, "B") }, null, null);
        SaturnCharacterTable table = new("Test", file);

        Assert.Single(table.Validate());
    }

    [Fact]
    public void Validate_reports_a_text_mapped_to_two_different_byte_sequences()
    {
        CharacterTableFile file = new(
            new List<CharacterTableEntry> { new(new byte[] { 0x41 }, "A"), new(new byte[] { 0x42 }, "A") }, null, null);
        SaturnCharacterTable table = new("Test", file);

        Assert.Single(table.Validate());
    }

    [Fact]
    public void Validate_returns_no_issue_for_a_consistent_table()
    {
        SaturnCharacterTable table = CreateAsciiTable();

        Assert.Empty(table.Validate());
    }

    [Fact]
    public void Name_is_exposed_as_constructed()
    {
        SaturnCharacterTable table = CreateAsciiTable();

        Assert.Equal("Test", table.Name);
    }
}
