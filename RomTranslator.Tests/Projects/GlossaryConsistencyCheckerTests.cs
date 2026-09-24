// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;
using RomTranslator.Core.Projects;
using Xunit;

namespace RomTranslator.Tests.Projects;

public sealed class GlossaryConsistencyCheckerTests
{
    [Fact]
    public void FindInconsistencies_flags_a_translation_that_does_not_use_the_retained_term()
    {
        TranslationEntry entry = new("e1", "Drink the Potion now", 0, translatedText: "Buvez le remède maintenant");
        List<GlossaryEntry> glossary = new() { new GlossaryEntry("Potion", "Fiole de soin") };

        IReadOnlyList<GlossaryInconsistency> inconsistencies = GlossaryConsistencyChecker.FindInconsistencies(new[] { entry }, glossary);

        GlossaryInconsistency inconsistency = Assert.Single(inconsistencies);
        Assert.Same(entry, inconsistency.Entry);
        Assert.Same(glossary[0], inconsistency.GlossaryEntry);
    }

    [Fact]
    public void FindInconsistencies_ignores_an_entry_using_the_retained_term()
    {
        TranslationEntry entry = new("e1", "Drink the Potion now", 0, translatedText: "Buvez la Fiole de soin maintenant");
        List<GlossaryEntry> glossary = new() { new GlossaryEntry("Potion", "Fiole de soin") };

        IReadOnlyList<GlossaryInconsistency> inconsistencies = GlossaryConsistencyChecker.FindInconsistencies(new[] { entry }, glossary);

        Assert.Empty(inconsistencies);
    }

    [Fact]
    public void FindInconsistencies_ignores_entries_that_are_not_yet_translated()
    {
        TranslationEntry entry = new("e1", "Drink the Potion now", 0);
        List<GlossaryEntry> glossary = new() { new GlossaryEntry("Potion", "Fiole de soin") };

        IReadOnlyList<GlossaryInconsistency> inconsistencies = GlossaryConsistencyChecker.FindInconsistencies(new[] { entry }, glossary);

        Assert.Empty(inconsistencies);
    }

    [Fact]
    public void FindInconsistencies_ignores_entries_whose_source_text_does_not_contain_the_term()
    {
        TranslationEntry entry = new("e1", "Drink the Elixir now", 0, translatedText: "Buvez l'élixir maintenant");
        List<GlossaryEntry> glossary = new() { new GlossaryEntry("Potion", "Fiole de soin") };

        IReadOnlyList<GlossaryInconsistency> inconsistencies = GlossaryConsistencyChecker.FindInconsistencies(new[] { entry }, glossary);

        Assert.Empty(inconsistencies);
    }
}
