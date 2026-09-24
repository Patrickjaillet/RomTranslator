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
    public void Constructor_rejects_an_occurrence_count_below_one()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TranslationEntry("e1", "Hello", 0, occurrenceCount: 0));
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
