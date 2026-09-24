// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Projects;
using RomTranslator.Modules.SegaSaturn.CharacterTables;
using RomTranslator.Modules.SegaSaturn.Translation;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.Translation;

public sealed class SaturnTranslationLengthPolicyTests
{
    private static SaturnCharacterTable CreateAsciiTable()
    {
        CharacterTableFile file = CharacterTableFileReader.Parse(new[] { "41=A", "42=B", "43=C" });
        return new SaturnCharacterTable("Test", file);
    }

    [Fact]
    public void Check_reports_the_source_encoded_length_as_the_limit()
    {
        SaturnTranslationLengthPolicy policy = new();
        TranslationEntry entry = new("e1", "AB", 0, translatedText: "A");

        TranslationLengthCheck check = policy.Check(entry, CreateAsciiTable());

        Assert.Equal(2, check.Limit);
        Assert.Equal(1, check.EncodedLength);
        Assert.False(check.ExceedsLimit);
    }

    [Fact]
    public void Check_flags_a_translation_longer_than_the_source()
    {
        SaturnTranslationLengthPolicy policy = new();
        TranslationEntry entry = new("e1", "A", 0, translatedText: "ABC");

        TranslationLengthCheck check = policy.Check(entry, CreateAsciiTable());

        Assert.True(check.ExceedsLimit);
    }

    [Fact]
    public void Check_does_not_throw_when_the_translation_contains_an_unsupported_character()
    {
        SaturnTranslationLengthPolicy policy = new();
        TranslationEntry entry = new("e1", "AB", 0, translatedText: "É");

        TranslationLengthCheck check = policy.Check(entry, CreateAsciiTable());

        Assert.True(check.ExceedsLimit);
    }
}
