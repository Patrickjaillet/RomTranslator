// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.App.ViewModels;
using RomTranslator.Core.Editing;
using RomTranslator.Core.Projects;
using Xunit;

namespace RomTranslator.Tests.ViewModels;

public sealed class FindReplaceViewModelTests
{
    [Fact]
    public void SearchText_updates_the_match_count()
    {
        TranslationEntry[] entries = { new("e1", "Hello", 0, translatedText: "Bonjour le monde") };
        FindReplaceViewModel viewModel = new(entries, new UndoRedoStack(), () => { });

        viewModel.SearchText = "monde";

        Assert.Equal(1, viewModel.MatchCount);
    }

    [Fact]
    public void ReplaceAllCommand_cannot_execute_with_an_empty_search_text()
    {
        TranslationEntry[] entries = { new("e1", "Hello", 0, translatedText: "Bonjour") };
        FindReplaceViewModel viewModel = new(entries, new UndoRedoStack(), () => { });

        Assert.False(viewModel.ReplaceAllCommand.CanExecute(null));
    }

    [Fact]
    public void ReplaceAllCommand_replaces_matches_and_notifies_the_callback()
    {
        TranslationEntry[] entries = { new("e1", "Hello", 0, translatedText: "Bonjour le monde") };
        UndoRedoStack undoRedo = new();
        bool notified = false;
        FindReplaceViewModel viewModel = new(entries, undoRedo, () => notified = true)
        {
            SearchText = "monde",
            ReplacementText = "univers",
        };

        viewModel.ReplaceAllCommand.Execute(null);

        Assert.Equal("Bonjour le univers", entries[0].TranslatedText);
        Assert.True(notified);
        Assert.True(undoRedo.CanUndo);
        Assert.Equal(0, viewModel.MatchCount);
    }

    [Fact]
    public void ReplaceAllCommand_with_no_match_reports_a_message_without_touching_the_undo_stack()
    {
        TranslationEntry[] entries = { new("e1", "Hello", 0, translatedText: "Bonjour") };
        UndoRedoStack undoRedo = new();
        FindReplaceViewModel viewModel = new(entries, undoRedo, () => { }) { SearchText = "introuvable" };

        viewModel.ReplaceAllCommand.Execute(null);

        Assert.False(undoRedo.CanUndo);
        Assert.False(string.IsNullOrEmpty(viewModel.ResultMessage));
    }
}
