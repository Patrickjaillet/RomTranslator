// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;
using RomTranslator.Modules.SegaSaturn.Disc;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.Disc;

public sealed class SectorReaderTests
{
    [Fact]
    public void ReadSector_reads_cooked_2048_byte_sectors()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "data.iso");
        byte[] sector0 = new byte[2048];
        Array.Fill(sector0, (byte)0xAA);
        byte[] sector1 = new byte[2048];
        Array.Fill(sector1, (byte)0xBB);
        File.WriteAllBytes(path, Concat(sector0, sector1));

        using SectorReader reader = new(new CueTrack(1, CueTrackMode.Mode1Cooked, path));

        Assert.All(reader.ReadSector(0), b => Assert.Equal(0xAA, b));
        Assert.All(reader.ReadSector(1), b => Assert.Equal(0xBB, b));
    }

    [Fact]
    public void ReadSector_skips_the_16_byte_header_of_raw_2352_byte_sectors()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "data.bin");
        byte[] rawSector = new byte[2352];
        Array.Fill(rawSector, (byte)0xFF, 0, 16);
        Array.Fill(rawSector, (byte)0xCC, 16, 2048);
        Array.Fill(rawSector, (byte)0xFF, 16 + 2048, 2352 - 16 - 2048);
        File.WriteAllBytes(path, rawSector);

        using SectorReader reader = new(new CueTrack(1, CueTrackMode.Mode1Raw, path));

        Assert.All(reader.ReadSector(0), b => Assert.Equal(0xCC, b));
    }

    [Fact]
    public void ReadBytes_spans_multiple_sectors()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "data.iso");
        byte[] sector0 = new byte[2048];
        Array.Fill(sector0, (byte)0x01);
        byte[] sector1 = new byte[2048];
        Array.Fill(sector1, (byte)0x02);
        File.WriteAllBytes(path, Concat(sector0, sector1));

        using SectorReader reader = new(new CueTrack(1, CueTrackMode.Mode1Cooked, path));
        byte[] result = reader.ReadBytes(0, 2048 + 100);

        Assert.Equal(2148, result.Length);
        Assert.Equal(0x01, result[0]);
        Assert.Equal(0x02, result[2048]);
    }

    [Fact]
    public void Constructor_rejects_an_audio_track()
    {
        Assert.Throws<ArgumentException>(() => new SectorReader(new CueTrack(1, CueTrackMode.Audio, "unused.wav")));
    }

    [Fact]
    public void ReadSector_throws_past_the_end_of_the_file()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "data.iso");
        File.WriteAllBytes(path, new byte[2048]);

        using SectorReader reader = new(new CueTrack(1, CueTrackMode.Mode1Cooked, path));

        Assert.Throws<EndOfStreamException>(() => reader.ReadSector(5));
    }

    private static byte[] Concat(byte[] first, byte[] second)
    {
        byte[] result = new byte[first.Length + second.Length];
        Array.Copy(first, result, first.Length);
        Array.Copy(second, 0, result, first.Length, second.Length);
        return result;
    }
}
