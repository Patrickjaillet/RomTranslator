// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.Core.Localization;
using Xunit;

namespace RomTranslator.Tests.Localization;

public sealed class LanguageSelectorTests
{
    [Theory]
    [InlineData("en", "fr", "en")]
    [InlineData("fr", "en", "fr")]
    [InlineData("de", "en", "en")]
    [InlineData(null, "en", "en")]
    [InlineData(null, "de", "fr")]
    [InlineData("", "en", "en")]
    public void Resolve_prefers_the_saved_language_then_the_system_language_then_French(
        string? savedLanguageCode, string systemLanguageCode, string expectedCode)
    {
        Assert.Equal(expectedCode, LanguageSelector.Resolve(savedLanguageCode, systemLanguageCode).TwoLetterISOLanguageName);
    }

    [Theory]
    [InlineData("fr", true)]
    [InlineData("en", true)]
    [InlineData("FR", true)]
    [InlineData("de", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void IsSupported_matches_only_the_declared_languages(string? languageCode, bool expected)
    {
        Assert.Equal(expected, LanguageSelector.IsSupported(languageCode));
    }

    [Fact]
    public void Resolve_rejects_a_null_system_language()
    {
        Assert.ThrowsAny<ArgumentException>(() => LanguageSelector.Resolve(null, null!));
    }
}
