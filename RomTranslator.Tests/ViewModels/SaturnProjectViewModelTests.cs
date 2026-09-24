// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.App.ViewModels;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Projects;
using RomTranslator.Modules.SegaSaturn.ViewModels;
using Xunit;

namespace RomTranslator.Tests.ViewModels;

public sealed class SaturnProjectViewModelTests
{
    private static TranslationEditorViewModel CreateEditor()
    {
        TranslationProject project = TranslationProjectFactory.Create("Projet de test", "sega-saturn", "rom.cue");
        return new TranslationEditorViewModel(project);
    }

    [Fact]
    public void HeaderText_shows_the_game_title_when_metadata_is_available()
    {
        SaturnRomInfoViewModel romInfo = new(new RomMetadata("Mon jeu", "Éditeur", "Europe", "T-12345"));
        SaturnProjectViewModel viewModel = new(romInfo, CreateEditor(), () => { }, () => { }, () => { });

        Assert.Equal("Mon jeu", viewModel.HeaderText);
        Assert.Same(romInfo, viewModel.RomInfo);
    }

    [Fact]
    public void HeaderText_falls_back_to_a_placeholder_when_metadata_is_unavailable()
    {
        SaturnProjectViewModel viewModel = new(null, CreateEditor(), () => { }, () => { }, () => { });

        Assert.False(string.IsNullOrEmpty(viewModel.HeaderText));
        Assert.Null(viewModel.RomInfo);
    }

    [Fact]
    public void Commands_invoke_the_supplied_actions()
    {
        int characterTableOpened = 0;
        int romExported = 0;
        int patchExported = 0;

        SaturnProjectViewModel viewModel = new(
            null, CreateEditor(), () => characterTableOpened++, () => romExported++, () => patchExported++);

        viewModel.OpenCharacterTableEditorCommand.Execute(null);
        viewModel.ExportTranslatedRomCommand.Execute(null);
        viewModel.ExportPatchCommand.Execute(null);

        Assert.Equal(1, characterTableOpened);
        Assert.Equal(1, romExported);
        Assert.Equal(1, patchExported);
    }
}
