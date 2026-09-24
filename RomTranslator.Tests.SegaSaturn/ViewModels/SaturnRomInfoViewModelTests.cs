// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.Core.Abstractions;
using RomTranslator.Modules.SegaSaturn.ViewModels;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.ViewModels;

public sealed class SaturnRomInfoViewModelTests
{
    [Fact]
    public void Exposes_the_metadata_fields_as_is()
    {
        RomMetadata metadata = new("MON JEU", "SEGA ENTERPRISES", "JTUE", "T-123456");

        SaturnRomInfoViewModel viewModel = new(metadata);

        Assert.Equal("MON JEU", viewModel.Title);
        Assert.Equal("SEGA ENTERPRISES", viewModel.Publisher);
        Assert.Equal("JTUE", viewModel.Region);
        Assert.Equal("T-123456", viewModel.ProductId);
    }

    [Fact]
    public void Preserves_null_optional_fields()
    {
        RomMetadata metadata = new("MON JEU", null, null, null);

        SaturnRomInfoViewModel viewModel = new(metadata);

        Assert.Null(viewModel.Publisher);
        Assert.Null(viewModel.Region);
        Assert.Null(viewModel.ProductId);
    }

    [Fact]
    public void Constructor_rejects_null()
    {
        Assert.Throws<ArgumentNullException>(() => new SaturnRomInfoViewModel(null!));
    }
}
