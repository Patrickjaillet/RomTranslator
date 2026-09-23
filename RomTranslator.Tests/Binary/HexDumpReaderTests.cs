// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.IO;
using System.Linq;
using RomTranslator.Core.Binary;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.Binary;

public sealed class HexDumpReaderTests
{
    private static string WriteFile(string directory, int length)
    {
        string path = Path.Combine(directory, "sample.bin");
        byte[] content = Enumerable.Range(0, length).Select(i => (byte)(i % 256)).ToArray();
        File.WriteAllBytes(path, content);
        return path;
    }

    [Fact]
    public void LineCount_rounds_up_to_the_next_full_line()
    {
        using TemporaryDirectory temp = new();
        HexDumpReader reader = new(WriteFile(temp.FullPath, 20));

        Assert.Equal(2, reader.LineCount);
    }

    [Fact]
    public void LineCount_is_zero_for_an_empty_file()
    {
        using TemporaryDirectory temp = new();
        HexDumpReader reader = new(WriteFile(temp.FullPath, 0));

        Assert.Equal(0, reader.LineCount);
    }

    [Fact]
    public void ReadLines_returns_full_lines_of_sixteen_bytes()
    {
        using TemporaryDirectory temp = new();
        HexDumpReader reader = new(WriteFile(temp.FullPath, 32));

        var lines = reader.ReadLines(0, 2);

        Assert.Equal(2, lines.Count);
        Assert.Equal(16, lines[0].Bytes.Length);
        Assert.Equal(0, lines[0].Offset);
        Assert.Equal(16, lines[1].Offset);
    }

    [Fact]
    public void ReadLines_truncates_the_last_line_of_the_file()
    {
        using TemporaryDirectory temp = new();
        HexDumpReader reader = new(WriteFile(temp.FullPath, 20));

        var lines = reader.ReadLines(0, 10);

        Assert.Equal(2, lines.Count);
        Assert.Equal(4, lines[1].Bytes.Length);
    }

    [Fact]
    public void ReadLines_starts_at_the_requested_line()
    {
        using TemporaryDirectory temp = new();
        HexDumpReader reader = new(WriteFile(temp.FullPath, 48));

        var lines = reader.ReadLines(1, 1);

        HexDumpLine only = Assert.Single(lines);
        Assert.Equal(16, only.Offset);
        Assert.Equal(16, only.Bytes[0]);
    }

    [Fact]
    public void ReadLines_returns_nothing_past_the_end_of_the_file()
    {
        using TemporaryDirectory temp = new();
        HexDumpReader reader = new(WriteFile(temp.FullPath, 16));

        Assert.Empty(reader.ReadLines(5, 3));
    }

    [Fact]
    public void ReadLines_rejects_a_negative_start_line()
    {
        using TemporaryDirectory temp = new();
        HexDumpReader reader = new(WriteFile(temp.FullPath, 16));

        Assert.Throws<System.ArgumentOutOfRangeException>(() => reader.ReadLines(-1, 1));
    }
}
