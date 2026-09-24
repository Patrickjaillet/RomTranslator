// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.IO;
using System.Linq;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Localization;
using RomTranslator.Modules.SegaSaturn.Disc;
using RomTranslator.Tests.SegaSaturn.Support;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.Disc;

public sealed class SaturnRomValidatorTests
{
    [Fact]
    public void Validate_accepts_a_well_formed_image_and_reports_the_detected_region()
    {
        using TemporaryDirectory temp = new();
        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath, areaSymbols: "JTUE      ");

        RomValidationResult result = new SaturnRomValidator().Validate(cuePath);

        Assert.True(result.IsValid);
        Assert.Contains(result.Messages, message => message.Contains("JTUE"));
    }

    [Fact]
    public void Validate_rejects_an_image_without_the_saturn_hardware_identifier()
    {
        using TemporaryDirectory temp = new();
        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath, validHardwareId: false);

        RomValidationResult result = new SaturnRomValidator().Validate(cuePath);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_reports_a_missing_data_file()
    {
        using TemporaryDirectory temp = new();
        string cuePath = Path.Combine(temp.FullPath, "missing.cue");
        File.WriteAllText(cuePath, "FILE \"absent.iso\" BINARY\n  TRACK 01 MODE1/2048\n    INDEX 01 00:00:00\n");

        RomValidationResult result = new SaturnRomValidator().Validate(cuePath);

        Assert.False(result.IsValid);
        string expectedFragment = Strings.Saturn_Validation_TrackFileMissing.Split("{0}")[0];
        Assert.Contains(result.Messages, message => message.Contains(expectedFragment));
    }

    [Fact]
    public void Validate_reports_a_truncated_data_file()
    {
        using TemporaryDirectory temp = new();
        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath);
        string isoPath = Path.Combine(temp.FullPath, "test.iso");
        byte[] truncated = File.ReadAllBytes(isoPath).Take(2048 + 100).ToArray();
        File.WriteAllBytes(isoPath, truncated);

        RomValidationResult result = new SaturnRomValidator().Validate(cuePath);

        Assert.False(result.IsValid);
        string expectedFragment = Strings.Saturn_Validation_TruncatedImage.Split("{0}")[0];
        Assert.Contains(result.Messages, message => message.Contains(expectedFragment));
    }

    [Fact]
    public void Validate_returns_false_when_the_cue_file_itself_is_invalid()
    {
        using TemporaryDirectory temp = new();
        string cuePath = Path.Combine(temp.FullPath, "invalid.cue");
        File.WriteAllText(cuePath, "not a cue sheet at all");

        RomValidationResult result = new SaturnRomValidator().Validate(cuePath);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Messages);
    }
}
