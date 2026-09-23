// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;
using System.IO;
using System.Linq;
using RomTranslator.App.ViewModels;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Projects;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.ViewModels;

public sealed class TranslationEditorViewModelTests
{
    private static TranslationProject CreateProject(params TranslationEntry[] entries)
    {
        TranslationProject project = TranslationProjectFactory.Create("Projet de test", "sega-saturn", "rom.cue");
        project.Entries.AddRange(entries);
        return project;
    }

    [Fact]
    public void Constructor_wraps_every_entry_and_selects_the_first_one()
    {
        TranslationProject project = CreateProject(new TranslationEntry("e1", "Hello", 0), new TranslationEntry("e2", "World", 8));
        TranslationEditorViewModel editor = new(project);

        Assert.Equal(2, editor.TotalCount);
        Assert.Equal("e1", editor.SelectedEntry?.Id);
    }

    [Fact]
    public void TranslatedCount_and_TranslatedPercent_reflect_the_status_of_the_entries()
    {
        TranslationProject project = CreateProject(
            new TranslationEntry("e1", "Hello", 0, status: TranslationStatus.Translated),
            new TranslationEntry("e2", "World", 8, status: TranslationStatus.NotTranslated),
            new TranslationEntry("e3", "Foo", 16, status: TranslationStatus.Validated),
            new TranslationEntry("e4", "Bar", 24, status: TranslationStatus.InProgress));
        TranslationEditorViewModel editor = new(project);

        Assert.Equal(2, editor.TranslatedCount);
        Assert.Equal(50.0, editor.TranslatedPercent);
        Assert.Contains("2", editor.ProgressSummaryText);
        Assert.Contains("4", editor.ProgressSummaryText);
    }

    [Fact]
    public void SearchText_filters_entries_by_source_or_translated_text()
    {
        TranslationProject project = CreateProject(
            new TranslationEntry("e1", "Hello world", 0),
            new TranslationEntry("e2", "Goodbye", 8, translatedText: "Au revoir"));
        TranslationEditorViewModel editor = new(project);

        editor.SearchText = "world";
        Assert.Single(editor.Entries.Cast<TranslationEntryViewModel>());

        editor.SearchText = "revoir";
        Assert.Single(editor.Entries.Cast<TranslationEntryViewModel>());

        editor.SearchText = string.Empty;
        Assert.Equal(2, editor.Entries.Cast<TranslationEntryViewModel>().Count());
    }

    [Fact]
    public void StatusFilter_shows_only_matching_entries()
    {
        TranslationProject project = CreateProject(
            new TranslationEntry("e1", "Hello", 0, status: TranslationStatus.NotTranslated),
            new TranslationEntry("e2", "World", 8, status: TranslationStatus.Validated));
        TranslationEditorViewModel editor = new(project);

        editor.StatusFilter = TranslationStatusFilter.Validated;

        TranslationEntryViewModel onlyMatch = Assert.Single(editor.Entries.Cast<TranslationEntryViewModel>());
        Assert.Equal("e2", onlyMatch.Id);
    }

    [Fact]
    public void StatusFilter_ExceedsLengthLimit_shows_only_entries_over_the_limit()
    {
        TranslationProject project = CreateProject(
            new TranslationEntry("e1", "Hi", 0, translatedText: "Bonjour"),
            new TranslationEntry("e2", "Hello", 8, translatedText: "Salut"));
        TranslationEditorViewModel editor = new(project, new FakeTranslationLengthPolicy(), new FakeCharacterTable());

        editor.StatusFilter = TranslationStatusFilter.ExceedsLengthLimit;

        TranslationEntryViewModel onlyMatch = Assert.Single(editor.Entries.Cast<TranslationEntryViewModel>());
        Assert.Equal("e1", onlyMatch.Id);
    }

    [Fact]
    public void UndoCommand_and_RedoCommand_reflect_the_stack_state()
    {
        TranslationProject project = CreateProject(new TranslationEntry("e1", "Hello", 0));
        TranslationEditorViewModel editor = new(project);

        Assert.False(editor.UndoCommand.CanExecute(null));

        editor.SelectedEntry!.TranslatedText = "Bonjour";
        Assert.True(editor.UndoCommand.CanExecute(null));

        editor.UndoCommand.Execute(null);
        Assert.Equal(string.Empty, editor.SelectedEntry!.TranslatedText);
        Assert.True(editor.RedoCommand.CanExecute(null));

        editor.RedoCommand.Execute(null);
        Assert.Equal("Bonjour", editor.SelectedEntry!.TranslatedText);
    }

    [Fact]
    public void Undoing_a_status_change_updates_the_progress_statistics()
    {
        TranslationProject project = CreateProject(new TranslationEntry("e1", "Hello", 0));
        TranslationEditorViewModel editor = new(project);

        editor.SelectedEntry!.Status = TranslationStatus.Validated;
        Assert.Equal(1, editor.TranslatedCount);

        editor.UndoCommand.Execute(null);
        Assert.Equal(0, editor.TranslatedCount);
    }

    [Fact]
    public void ExportCsv_then_ImportCsv_round_trips_a_reviewed_translation()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "export.csv");
        TranslationProject project = CreateProject(new TranslationEntry("e1", "Hello", 0, translatedText: "Bonjour"));
        TranslationEditorViewModel editor = new(project);

        editor.ExportCsv(path);
        project.Entries[0].TranslatedText = string.Empty;
        int updated = editor.ImportCsv(path);

        Assert.Equal(1, updated);
        Assert.Equal("Bonjour", editor.SelectedEntry!.TranslatedText);
    }

    [Fact]
    public void Constructor_rejects_null()
    {
        Assert.Throws<System.ArgumentNullException>(() => new TranslationEditorViewModel(null!));
    }
}
