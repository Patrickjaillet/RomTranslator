// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.IO;
using RomTranslator.Core.Abstractions;
using RomTranslator.Modules.SegaSaturn.Disc;
using RomTranslator.Tests.SegaSaturn.Support;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.Disc;

public sealed class SaturnRomLoaderTests
{
    [Fact]
    public void Load_reads_the_metadata_from_the_IP_BIN_header()
    {
        using TemporaryDirectory temp = new();
        string cuePath = SyntheticSaturnDiscBuilder.Build(
            temp.FullPath, gameTitle: "MON JEU", makerId: "SEGA ENTERPRISES", productNumber: "T-123456  ", areaSymbols: "JTUE      ");

        SaturnRomLoader loader = new();
        RomMetadata metadata = loader.Load(cuePath);

        Assert.Equal("MON JEU", metadata.Title);
        Assert.Equal("SEGA ENTERPRISES", metadata.Publisher);
        Assert.Equal("JTUE", metadata.Region);
        Assert.Equal("T-123456", metadata.ProductId);
    }

    [Fact]
    public void Load_throws_when_the_hardware_identifier_is_missing()
    {
        using TemporaryDirectory temp = new();
        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath, validHardwareId: false);

        SaturnRomLoader loader = new();

        Assert.Throws<InvalidDataException>(() => loader.Load(cuePath));
    }

    [Fact]
    public void Load_throws_when_the_cue_file_has_no_data_track()
    {
        using TemporaryDirectory temp = new();
        string cuePath = Path.Combine(temp.FullPath, "audio-only.cue");
        File.WriteAllText(cuePath, "FILE \"a.wav\" WAVE\n  TRACK 01 AUDIO\n    INDEX 01 00:00:00\n");

        SaturnRomLoader loader = new();

        Assert.Throws<InvalidDataException>(() => loader.Load(cuePath));
    }
}
