// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.Modules.SegaSaturn.CharacterTables;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.CharacterTables;

public sealed class CharacterTableEntryViewModelTests
{
    [Fact]
    public void Constructor_from_entry_populates_hex_and_text()
    {
        CharacterTableEntryViewModel viewModel = new(new CharacterTableEntry(new byte[] { 0x41 }, "A"));

        Assert.Equal("41", viewModel.HexBytes);
        Assert.Equal("A", viewModel.Text);
        Assert.True(viewModel.IsValid);
    }

    [Fact]
    public void Default_constructor_starts_empty_and_invalid()
    {
        CharacterTableEntryViewModel viewModel = new();

        Assert.Equal(string.Empty, viewModel.HexBytes);
        Assert.False(viewModel.IsValid);
    }

    [Theory]
    [InlineData("4", false)]
    [InlineData("ZZ", false)]
    [InlineData("", false)]
    [InlineData("41", true)]
    [InlineData("4142", true)]
    public void IsValid_reflects_the_syntax_of_HexBytes(string hex, bool expectedValid)
    {
        CharacterTableEntryViewModel viewModel = new() { HexBytes = hex, Text = "x" };

        Assert.Equal(expectedValid, viewModel.IsValid);
    }

    [Fact]
    public void ToEntry_converts_hex_bytes_and_text()
    {
        CharacterTableEntryViewModel viewModel = new() { HexBytes = "4142", Text = "TH" };

        CharacterTableEntry entry = viewModel.ToEntry();

        Assert.Equal(new byte[] { 0x41, 0x42 }, entry.Bytes);
        Assert.Equal("TH", entry.Text);
    }

    [Fact]
    public void ToEntry_throws_when_the_hex_sequence_is_invalid()
    {
        CharacterTableEntryViewModel viewModel = new() { HexBytes = "Z", Text = "x" };

        Assert.Throws<FormatException>(() => viewModel.ToEntry());
    }
}
