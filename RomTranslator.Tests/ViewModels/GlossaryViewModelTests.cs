// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Linq;
using RomTranslator.App.ViewModels;
using RomTranslator.Core.Projects;
using Xunit;

namespace RomTranslator.Tests.ViewModels;

public sealed class GlossaryViewModelTests
{
    private static TranslationProject CreateProject(params TranslationEntry[] entries)
    {
        TranslationProject project = TranslationProjectFactory.Create("Projet de test", "sega-saturn", "rom.cue");
        project.Entries.AddRange(entries);
        return project;
    }

    [Fact]
    public void Constructor_wraps_the_existing_glossary_entries()
    {
        TranslationProject project = CreateProject();
        project.Glossary.Add(new GlossaryEntry("Potion", "Fiole de soin"));

        GlossaryViewModel viewModel = new(project);

        GlossaryEntryViewModel onlyEntry = Assert.Single(viewModel.Entries);
        Assert.Equal("Potion", onlyEntry.SourceTerm);
    }

    [Fact]
    public void AddEntryCommand_adds_a_new_term_to_the_project_and_selects_it()
    {
        TranslationProject project = CreateProject();
        GlossaryViewModel viewModel = new(project);

        viewModel.AddEntryCommand.Execute("Potion");

        Assert.Single(project.Glossary);
        Assert.Equal("Potion", viewModel.SelectedEntry?.SourceTerm);
    }

    [Fact]
    public void AddEntryCommand_cannot_execute_for_a_blank_term()
    {
        GlossaryViewModel viewModel = new(CreateProject());

        Assert.False(viewModel.AddEntryCommand.CanExecute("   "));
    }

    [Fact]
    public void RemoveEntryCommand_removes_the_selected_entry_from_the_project()
    {
        TranslationProject project = CreateProject();
        project.Glossary.Add(new GlossaryEntry("Potion", "Fiole de soin"));
        GlossaryViewModel viewModel = new(project);
        viewModel.SelectedEntry = viewModel.Entries.Single();

        viewModel.RemoveEntryCommand.Execute(null);

        Assert.Empty(project.Glossary);
        Assert.Empty(viewModel.Entries);
        Assert.Null(viewModel.SelectedEntry);
    }

    [Fact]
    public void RemoveEntryCommand_cannot_execute_without_a_selection()
    {
        GlossaryViewModel viewModel = new(CreateProject());

        Assert.False(viewModel.RemoveEntryCommand.CanExecute(null));
    }

    [Fact]
    public void RefreshInconsistencies_reflects_the_current_glossary_and_translations()
    {
        TranslationProject project = CreateProject(new TranslationEntry("e1", "Drink the Potion", 0, translatedText: "Buvez le remède"));
        project.Glossary.Add(new GlossaryEntry("Potion", "Fiole de soin"));

        GlossaryViewModel viewModel = new(project);

        Assert.True(viewModel.HasInconsistencies);
        Assert.Single(viewModel.Inconsistencies);
    }
}
