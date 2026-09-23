// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;
using System.IO;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Projects;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.Projects;

public sealed class TranslationCsvExchangeTests
{
    [Fact]
    public void Export_writes_a_header_and_one_row_per_entry()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "export.csv");
        List<TranslationEntry> entries = new()
        {
            new TranslationEntry("e1", "Hello", 0, translatedText: "Bonjour", status: TranslationStatus.Translated),
            new TranslationEntry("e2", "World", 8, translatedText: "Monde", status: TranslationStatus.Validated, context: "Titre"),
        };

        TranslationCsvExchange.Export(entries, path);
        string[] lines = File.ReadAllLines(path);

        Assert.Equal("id,sourceText,translatedText,status,context", lines[0]);
        Assert.Equal(3, lines.Length);
        Assert.Contains("e1,Hello,Bonjour,Translated,", lines[1]);
        Assert.Contains("e2,World,Monde,Validated,Titre", lines[2]);
    }

    [Fact]
    public void Export_quotes_fields_that_contain_a_comma_or_a_quote()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "export.csv");
        List<TranslationEntry> entries = new()
        {
            new TranslationEntry("e1", "Bonjour, \"ami\"", 0),
        };

        TranslationCsvExchange.Export(entries, path);
        string content = File.ReadAllText(path);

        Assert.Contains("\"Bonjour, \"\"ami\"\"\"", content);
    }

    [Fact]
    public void Import_applies_the_reviewed_translation_by_matching_id()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "review.csv");
        File.WriteAllText(path, "id,sourceText,translatedText,status,context\ne1,Hello,Bonjour relu,Translated,\n");
        List<TranslationEntry> entries = new() { new TranslationEntry("e1", "Hello", 0, translatedText: "Bonjour") };

        int updated = TranslationCsvExchange.Import(entries, path);

        Assert.Equal(1, updated);
        Assert.Equal("Bonjour relu", entries[0].TranslatedText);
    }

    [Fact]
    public void Import_ignores_rows_whose_id_is_unknown()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "review.csv");
        File.WriteAllText(path, "id,sourceText,translatedText,status,context\ninconnu,Hello,Bonjour,Translated,\n");
        List<TranslationEntry> entries = new() { new TranslationEntry("e1", "Hello", 0) };

        int updated = TranslationCsvExchange.Import(entries, path);

        Assert.Equal(0, updated);
        Assert.Equal(string.Empty, entries[0].TranslatedText);
    }

    [Fact]
    public void Import_reads_a_quoted_field_containing_a_comma()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "review.csv");
        File.WriteAllText(path, "id,sourceText,translatedText,status,context\ne1,Hello,\"Bonjour, cher ami\",Translated,\n");
        List<TranslationEntry> entries = new() { new TranslationEntry("e1", "Hello", 0) };

        TranslationCsvExchange.Import(entries, path);

        Assert.Equal("Bonjour, cher ami", entries[0].TranslatedText);
    }

    [Fact]
    public void Import_throws_when_the_header_does_not_match()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "invalide.csv");
        File.WriteAllText(path, "colonne1,colonne2\n");

        Assert.Throws<InvalidDataException>(() => TranslationCsvExchange.Import(new List<TranslationEntry>(), path));
    }

    [Fact]
    public void Export_then_Import_round_trips_a_translation_containing_a_newline()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "export.csv");
        List<TranslationEntry> entries = new() { new TranslationEntry("e1", "Hello", 0, translatedText: "Ligne 1\nLigne 2") };

        TranslationCsvExchange.Export(entries, path);
        entries[0].TranslatedText = string.Empty;
        TranslationCsvExchange.Import(entries, path);

        Assert.Equal("Ligne 1\nLigne 2", entries[0].TranslatedText);
    }
}
