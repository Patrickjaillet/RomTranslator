// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;
using RomTranslator.Modules.SegaSaturn.Disc;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.Disc;

public sealed class SectorWriterTests
{
    [Fact]
    public void WriteBytes_writes_within_a_single_cooked_sector()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "data.iso");
        File.WriteAllBytes(path, new byte[2048 * 2]);

        using (SectorWriter writer = new(new CueTrack(1, CueTrackMode.Mode1Cooked, path)))
        {
            writer.WriteBytes(10, new byte[] { 0x41, 0x42, 0x43 });
        }

        using SectorReader reader = new(new CueTrack(1, CueTrackMode.Mode1Cooked, path));
        byte[] sector = reader.ReadSector(0);

        Assert.Equal(0x41, sector[10]);
        Assert.Equal(0x42, sector[11]);
        Assert.Equal(0x43, sector[12]);
    }

    [Fact]
    public void WriteBytes_spans_multiple_cooked_sectors()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "data.iso");
        File.WriteAllBytes(path, new byte[2048 * 2]);

        byte[] payload = new byte[10];
        Array.Fill(payload, (byte)0xAB);

        using (SectorWriter writer = new(new CueTrack(1, CueTrackMode.Mode1Cooked, path)))
        {
            writer.WriteBytes(2045, payload);
        }

        using SectorReader reader = new(new CueTrack(1, CueTrackMode.Mode1Cooked, path));
        byte[] result = reader.ReadBytes(0, 2048 * 2);

        for (int i = 0; i < payload.Length; i++)
        {
            Assert.Equal(0xAB, result[2045 + i]);
        }
    }

    [Fact]
    public void WriteBytes_writes_past_the_16_byte_header_of_raw_sectors()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "data.bin");
        byte[] rawSector = new byte[2352];
        Array.Fill(rawSector, (byte)0xFF, 0, 16);
        File.WriteAllBytes(path, rawSector);

        using (SectorWriter writer = new(new CueTrack(1, CueTrackMode.Mode1Raw, path)))
        {
            writer.WriteBytes(0, new byte[] { 0x11, 0x22 });
        }

        using SectorReader reader = new(new CueTrack(1, CueTrackMode.Mode1Raw, path));
        byte[] sector = reader.ReadSector(0);

        Assert.Equal(0x11, sector[0]);
        Assert.Equal(0x22, sector[1]);
    }

    [Fact]
    public void Constructor_rejects_an_audio_track()
    {
        Assert.Throws<ArgumentException>(() => new SectorWriter(new CueTrack(1, CueTrackMode.Audio, "unused.wav")));
    }
}
