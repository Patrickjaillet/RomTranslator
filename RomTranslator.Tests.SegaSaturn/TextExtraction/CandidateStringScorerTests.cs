// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.Modules.SegaSaturn.TextExtraction;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.TextExtraction;

public sealed class CandidateStringScorerTests
{
    [Fact]
    public void Score_is_one_for_plain_english_text()
    {
        Assert.Equal(1.0, CandidateStringScorer.Score("Hello, World!"));
    }

    [Fact]
    public void Score_is_one_for_plain_french_text_with_accents()
    {
        Assert.Equal(1.0, CandidateStringScorer.Score("Ça va très bien, merci."));
    }

    [Fact]
    public void Score_is_zero_for_the_replacement_character()
    {
        Assert.Equal(0.0, CandidateStringScorer.Score("���"));
    }

    [Fact]
    public void Score_is_zero_for_an_empty_string()
    {
        Assert.Equal(0.0, CandidateStringScorer.Score(string.Empty));
    }

    [Fact]
    public void Score_is_partial_for_a_mix_of_plausible_and_implausible_characters()
    {
        double score = CandidateStringScorer.Score("AB\u0001\u0002");

        Assert.Equal(0.5, score);
    }

    [Fact]
    public void Score_counts_digits_and_common_punctuation_as_plausible()
    {
        Assert.Equal(1.0, CandidateStringScorer.Score("Room 42 - \"exit\"?"));
    }
}
