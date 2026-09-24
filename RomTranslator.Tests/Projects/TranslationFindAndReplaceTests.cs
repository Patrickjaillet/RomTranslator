// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.Core.Editing;
using RomTranslator.Core.Projects;
using Xunit;

namespace RomTranslator.Tests.Projects;

public sealed class TranslationFindAndReplaceTests
{
    [Fact]
    public void CountMatches_counts_only_entries_whose_translation_contains_the_search_text()
    {
        TranslationEntry[] entries =
        {
            new("e1", "Hello", 0, translatedText: "Bonjour le monde"),
            new("e2", "World", 8, translatedText: "Le monde perdu"),
            new("e3", "Foo", 16, translatedText: "Autre chose"),
        };

        Assert.Equal(2, TranslationFindAndReplace.CountMatches(entries, "monde"));
    }

    [Fact]
    public void CountMatches_is_case_sensitive()
    {
        TranslationEntry[] entries = { new("e1", "Hello", 0, translatedText: "Monde perdu") };

        Assert.Equal(0, TranslationFindAndReplace.CountMatches(entries, "monde"));
    }

    [Fact]
    public void PrepareReplaceAll_returns_null_when_nothing_matches()
    {
        TranslationEntry[] entries = { new("e1", "Hello", 0, translatedText: "Bonjour") };

        CompositeEditCommand? command = TranslationFindAndReplace.PrepareReplaceAll(entries, "introuvable", "x");

        Assert.Null(command);
    }

    [Fact]
    public void PrepareReplaceAll_replaces_the_search_text_in_every_matching_entry()
    {
        TranslationEntry[] entries =
        {
            new("e1", "Hello", 0, translatedText: "Bonjour le monde"),
            new("e2", "World", 8, translatedText: "Monde perdu"),
        };

        CompositeEditCommand? command = TranslationFindAndReplace.PrepareReplaceAll(entries, "monde", "univers");
        Assert.NotNull(command);
        Assert.Equal(1, command!.Count);

        command.Do();

        Assert.Equal("Bonjour le univers", entries[0].TranslatedText);
        Assert.Equal("Monde perdu", entries[1].TranslatedText);
    }

    [Fact]
    public void PrepareReplaceAll_command_is_reversible()
    {
        TranslationEntry[] entries = { new("e1", "Hello", 0, translatedText: "Bonjour le monde") };

        CompositeEditCommand? command = TranslationFindAndReplace.PrepareReplaceAll(entries, "monde", "univers");
        Assert.NotNull(command);

        command!.Do();
        command.Undo();

        Assert.Equal("Bonjour le monde", entries[0].TranslatedText);
    }
}
