// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.IO;
using RomTranslator.Modules.SegaSaturn.CharacterTables;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.CharacterTables;

public sealed class CharacterTableFileReaderTests
{
    [Fact]
    public void Parse_reads_single_byte_entries()
    {
        CharacterTableFile file = CharacterTableFileReader.Parse(new[] { "41=A", "42=B" });

        Assert.Equal(2, file.Entries.Count);
        Assert.Equal(new byte[] { 0x41 }, file.Entries[0].Bytes);
        Assert.Equal("A", file.Entries[0].Text);
    }

    [Fact]
    public void Parse_reads_multi_byte_entries_for_DTE_MTE()
    {
        CharacterTableFile file = CharacterTableFileReader.Parse(new[] { "1502=that" });

        CharacterTableEntry entry = Assert.Single(file.Entries);
        Assert.Equal(new byte[] { 0x15, 0x02 }, entry.Bytes);
        Assert.Equal("that", entry.Text);
    }

    [Fact]
    public void Parse_ignores_blank_lines_and_comments()
    {
        CharacterTableFile file = CharacterTableFileReader.Parse(new[] { "# comment", "", "41=A", "  " });

        Assert.Single(file.Entries);
    }

    [Fact]
    public void Parse_reads_the_new_line_marker()
    {
        CharacterTableFile file = CharacterTableFileReader.Parse(new[] { "41=A", "*FE" });

        Assert.Equal(new byte[] { 0xFE }, file.NewLineBytes);
    }

    [Fact]
    public void Parse_reads_the_end_of_text_marker_with_or_without_a_label()
    {
        CharacterTableFile withLabel = CharacterTableFileReader.Parse(new[] { "\\FF=[end of text]" });
        CharacterTableFile withoutLabel = CharacterTableFileReader.Parse(new[] { "\\FF" });

        Assert.Equal(new byte[] { 0xFF }, withLabel.EndOfTextBytes);
        Assert.Equal(new byte[] { 0xFF }, withoutLabel.EndOfTextBytes);
    }

    [Fact]
    public void Parse_throws_on_a_line_without_an_equals_sign()
    {
        Assert.Throws<InvalidDataException>(() => CharacterTableFileReader.Parse(new[] { "not a valid line" }));
    }

    [Fact]
    public void Parse_throws_on_an_odd_length_hex_sequence()
    {
        Assert.Throws<InvalidDataException>(() => CharacterTableFileReader.Parse(new[] { "4=A" }));
    }

    [Fact]
    public void Parse_throws_on_a_non_hexadecimal_sequence()
    {
        Assert.Throws<InvalidDataException>(() => CharacterTableFileReader.Parse(new[] { "ZZ=A" }));
    }

    [Fact]
    public void Parse_preserves_a_space_character_as_the_text_value()
    {
        CharacterTableFile file = CharacterTableFileReader.Parse(new[] { "20= " });

        Assert.Equal(" ", Assert.Single(file.Entries).Text);
    }
}
