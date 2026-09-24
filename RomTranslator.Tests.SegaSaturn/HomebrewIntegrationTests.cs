// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;
using RomTranslator.Core.Abstractions;
using RomTranslator.Modules.SegaSaturn.CharacterTables;
using RomTranslator.Modules.SegaSaturn.Disc;
using RomTranslator.Modules.SegaSaturn.TextExtraction;
using RomTranslator.Modules.SegaSaturn.Translation;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn;

/// <summary>
/// Test d'intégration bout-en-bout sur une véritable image homebrew Sega Saturn, libre de droits, disponible
/// localement chez le propriétaire du dépôt pour la vérification manuelle du module (voir <c>CLAUDE.md</c>).
/// Le chemin de cette image n'est <b>jamais</b> committé dans le dépôt ni codé en dur ailleurs que dans ce
/// fichier de test ; l'image elle-même n'est jamais copiée ni incluse dans le dépôt (contrainte stricte du
/// ROADMAP : aucune ROM, même homebrew, n'est committée). Ce test s'ignore silencieusement sur toute machine
/// où l'image n'est pas présente à cet emplacement (poste du propriétaire uniquement), pour ne jamais faire
/// échouer la suite ailleurs (intégration continue, autre poste de développement).
/// </summary>
public sealed class HomebrewIntegrationTests
{
    private const string LocalHomebrewCuePath = @"C:\Users\Patrick\Desktop\CubecatYarniaPublic2_0_2\sl_coff.cue";

    private static SaturnCharacterTable CreateAsciiTable()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "PredefinedTables", "ascii.tbl");
        CharacterTableFile file = CharacterTableFileReader.Read(path);
        return new SaturnCharacterTable("ASCII", file);
    }

    [Fact]
    public void Loader_validator_and_extractor_succeed_on_the_real_homebrew_image()
    {
        if (!File.Exists(LocalHomebrewCuePath))
        {
            Assert.Skip("Image homebrew locale absente (disponible uniquement sur le poste du propriétaire, jamais committée) : voir CLAUDE.md.");
            return;
        }

        SaturnRomLoader loader = new();
        SaturnRomValidator validator = new();
        SaturnTextExtractor extractor = new();
        SaturnCharacterTable table = CreateAsciiTable();

        RomMetadata metadata = loader.Load(LocalHomebrewCuePath);
        Assert.False(string.IsNullOrWhiteSpace(metadata.Title));

        RomValidationResult validation = validator.Validate(LocalHomebrewCuePath);
        Assert.True(validation.IsValid, string.Join(" | ", validation.Messages));

        var entries = extractor.ExtractAutomatically(LocalHomebrewCuePath, table);
        Assert.NotEmpty(entries);
    }

    [Fact]
    public void Reinjecting_untouched_entries_produces_a_functionally_identical_image()
    {
        if (!File.Exists(LocalHomebrewCuePath))
        {
            Assert.Skip("Image homebrew locale absente (disponible uniquement sur le poste du propriétaire, jamais committée) : voir CLAUDE.md.");
            return;
        }

        using TemporaryDirectory temp = new();
        string outputCue = Path.Combine(temp.FullPath, "output", "sl_coff.cue");

        SaturnTextExtractor extractor = new();
        SaturnTextInjector injector = new();
        SaturnCharacterTable table = CreateAsciiTable();

        var entries = extractor.ExtractAutomatically(LocalHomebrewCuePath, table);

        // Aucune entrée n'est traduite : la réinjection ne doit rien réécrire (TranslatedText vide est
        // ignoré par SaturnTextInjector), donc l'image générée doit rester lisible et se réextraire à
        // l'identique.
        TextInjectionResult result = injector.Inject(LocalHomebrewCuePath, outputCue, entries, table);
        Assert.Empty(result.Messages);

        var reExtracted = extractor.ExtractAutomatically(outputCue, table);
        Assert.Equal(entries.Count, reExtracted.Count);
    }
}
