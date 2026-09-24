// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.Core.Projects;
using Xunit;

namespace RomTranslator.Tests.Projects;

public sealed class GlossaryEntryTests
{
    [Fact]
    public void Constructor_applies_the_defaults()
    {
        GlossaryEntry entry = new("Potion", "Potion");

        Assert.Equal("Potion", entry.SourceTerm);
        Assert.Equal("Potion", entry.TargetTerm);
        Assert.Null(entry.Note);
    }

    [Fact]
    public void TargetTerm_and_Note_are_mutable()
    {
        GlossaryEntry entry = new("Potion", "Potion");

        entry.TargetTerm = "Fiole de soin";
        entry.Note = "Toujours traduit ainsi depuis le chapitre 2.";

        Assert.Equal("Fiole de soin", entry.TargetTerm);
        Assert.Equal("Toujours traduit ainsi depuis le chapitre 2.", entry.Note);
    }
}
