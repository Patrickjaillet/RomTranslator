// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Projects;
using Xunit;

namespace RomTranslator.Tests.Projects;

public sealed class TranslationEntryTests
{
    [Fact]
    public void Constructor_applies_the_defaults()
    {
        TranslationEntry entry = new("e1", "Hello", 0x100);

        Assert.Equal("e1", entry.Id);
        Assert.Equal("Hello", entry.SourceText);
        Assert.Equal(0x100, entry.Offset);
        Assert.Equal(1, entry.OccurrenceCount);
        Assert.Null(entry.Context);
        Assert.Equal(string.Empty, entry.TranslatedText);
        Assert.Equal(TranslationStatus.NotTranslated, entry.Status);
    }

    [Fact]
    public void TranslatedText_and_Status_are_mutable()
    {
        TranslationEntry entry = new("e1", "Hello", 0);

        entry.TranslatedText = "Bonjour";
        entry.Status = TranslationStatus.Translated;

        Assert.Equal("Bonjour", entry.TranslatedText);
        Assert.Equal(TranslationStatus.Translated, entry.Status);
    }

    [Fact]
    public void Constructor_rejects_an_empty_offset_list()
    {
        Assert.Throws<ArgumentException>(() => new TranslationEntry("e1", "Hello", Array.Empty<long>()));
    }

    [Fact]
    public void Constructor_accepts_multiple_offsets_for_duplicated_occurrences()
    {
        TranslationEntry entry = new("e1", "Hello", new long[] { 0x10, 0x40, 0x90 });

        Assert.Equal(0x10, entry.Offset);
        Assert.Equal(3, entry.OccurrenceCount);
        Assert.Equal(new long[] { 0x10, 0x40, 0x90 }, entry.Offsets);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_rejects_an_invalid_id(string? id)
    {
        Assert.ThrowsAny<ArgumentException>(() => new TranslationEntry(id!, "Hello", 0));
    }
}
