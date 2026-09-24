// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;
using System.IO;
using RomTranslator.Modules.SegaSaturn.CharacterTables;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.CharacterTables;

public sealed class CharacterTableFileWriterTests
{
    [Fact]
    public void Write_then_Read_round_trips_entries_and_markers()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "roundtrip.tbl");

        CharacterTableFile original = new(
            new List<CharacterTableEntry>
            {
                new(new byte[] { 0x41 }, "A"),
                new(new byte[] { 0x15, 0x02 }, "that"),
            },
            NewLineBytes: new byte[] { 0xFE },
            EndOfTextBytes: new byte[] { 0xFF });

        CharacterTableFileWriter.Write(original, path);
        CharacterTableFile reloaded = CharacterTableFileReader.Read(path);

        Assert.Equal(2, reloaded.Entries.Count);
        Assert.Equal("A", reloaded.Entries[0].Text);
        Assert.Equal("that", reloaded.Entries[1].Text);
        Assert.Equal(new byte[] { 0xFE }, reloaded.NewLineBytes);
        Assert.Equal(new byte[] { 0xFF }, reloaded.EndOfTextBytes);
    }

    [Fact]
    public void Write_omits_markers_when_absent()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "no-markers.tbl");
        CharacterTableFile table = new(new List<CharacterTableEntry> { new(new byte[] { 0x41 }, "A") }, null, null);

        CharacterTableFileWriter.Write(table, path);
        CharacterTableFile reloaded = CharacterTableFileReader.Read(path);

        Assert.Null(reloaded.NewLineBytes);
        Assert.Null(reloaded.EndOfTextBytes);
    }
}
