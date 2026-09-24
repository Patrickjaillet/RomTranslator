// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.Modules.SegaSaturn.TextExtraction;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.TextExtraction;

public sealed class TextScanOptionsTests
{
    [Fact]
    public void Default_values_pass_validation()
    {
        Exception? exception = Record.Exception(() => TextScanOptions.Default.Validate());

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_rejects_a_minimum_length_below_one()
    {
        TextScanOptions options = new(MinimumLength: 0, MinimumScore: 0.5);

        Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Validate_rejects_a_score_outside_zero_to_one(double score)
    {
        TextScanOptions options = new(MinimumLength: 4, MinimumScore: score);

        Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
    }
}
