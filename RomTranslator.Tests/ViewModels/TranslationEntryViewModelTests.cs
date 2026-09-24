// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RomTranslator.App.ViewModels;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Editing;
using RomTranslator.Core.Projects;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.ViewModels;

public sealed class TranslationEntryViewModelTests
{
    /// <summary>Table de caractères stricte (rejette tout caractère hors ASCII), pour tester l'aperçu.</summary>
    private sealed class StrictAsciiCharacterTable : ICharacterTable
    {
        public string Name => "ASCII strict de test";

        public string Decode(IReadOnlyList<byte> bytes) => Encoding.ASCII.GetString(bytes.ToArray());

        public IReadOnlyList<byte> Encode(string text)
        {
            if (text.Any(c => c > 127))
            {
                throw new ArgumentException("Caractère non pris en charge.", nameof(text));
            }

            return Encoding.ASCII.GetBytes(text);
        }

        public IReadOnlyList<string> Validate() => Array.Empty<string>();
    }

    [Fact]
    public void Exposes_the_read_only_fields_of_the_wrapped_entry()
    {
        TranslationEntry entry = new("e1", "Hello", new long[] { 0x1A, 0x2A, 0x3A }, context: "Titre de l'écran");
        TranslationEntryViewModel viewModel = new(entry, new UndoRedoStack());

        Assert.Equal("e1", viewModel.Id);
        Assert.Equal("Hello", viewModel.SourceText);
        Assert.Equal("Titre de l'écran", viewModel.Context);
        Assert.Equal("0x1A", viewModel.OffsetDisplay);
        Assert.Equal(3, viewModel.OccurrenceCount);
    }

    [Fact]
    public void Setting_TranslatedText_updates_the_entry_through_the_undo_redo_stack()
    {
        TranslationEntry entry = new("e1", "Hello", 0);
        UndoRedoStack undoRedo = new();
        TranslationEntryViewModel viewModel = new(entry, undoRedo);

        viewModel.TranslatedText = "Bonjour";

        Assert.Equal("Bonjour", entry.TranslatedText);
        Assert.True(undoRedo.CanUndo);
    }

    [Fact]
    public void Setting_TranslatedText_to_the_same_value_does_not_push_a_command()
    {
        TranslationEntry entry = new("e1", "Hello", 0, translatedText: "Bonjour");
        UndoRedoStack undoRedo = new();
        TranslationEntryViewModel viewModel = new(entry, undoRedo);

        viewModel.TranslatedText = "Bonjour";

        Assert.False(undoRedo.CanUndo);
    }

    [Fact]
    public void Setting_Status_updates_the_entry_through_the_undo_redo_stack()
    {
        TranslationEntry entry = new("e1", "Hello", 0);
        UndoRedoStack undoRedo = new();
        TranslationEntryViewModel viewModel = new(entry, undoRedo);

        viewModel.Status = TranslationStatus.Validated;

        Assert.Equal(TranslationStatus.Validated, entry.Status);
        Assert.True(undoRedo.CanUndo);
    }

    [Fact]
    public void LengthCheck_is_null_when_no_policy_is_configured()
    {
        TranslationEntry entry = new("e1", "Hello", 0);
        TranslationEntryViewModel viewModel = new(entry, new UndoRedoStack());

        Assert.Null(viewModel.LengthCheck);
        Assert.False(viewModel.ExceedsLengthLimit);
        Assert.Null(viewModel.LengthLimitDisplayText);
        Assert.Null(viewModel.LengthLimitExceededMessage);
    }

    [Fact]
    public void ExceedsLengthLimit_is_true_when_the_translation_is_longer_than_the_source()
    {
        TranslationEntry entry = new("e1", "Hi", 0, translatedText: "Bonjour");
        TranslationEntryViewModel viewModel = new(entry, new UndoRedoStack(), new FakeTranslationLengthPolicy(), new FakeCharacterTable());

        Assert.True(viewModel.ExceedsLengthLimit);
        Assert.NotNull(viewModel.LengthLimitDisplayText);
        Assert.NotNull(viewModel.LengthLimitExceededMessage);
    }

    [Fact]
    public void ExceedsLengthLimit_is_false_when_the_translation_fits()
    {
        TranslationEntry entry = new("e1", "Hello", 0, translatedText: "Salut");
        TranslationEntryViewModel viewModel = new(entry, new UndoRedoStack(), new FakeTranslationLengthPolicy(), new FakeCharacterTable());

        Assert.False(viewModel.ExceedsLengthLimit);
        Assert.Null(viewModel.LengthLimitExceededMessage);
    }

    [Fact]
    public void CharacterTablePreviewText_is_null_when_no_character_table_is_configured()
    {
        TranslationEntry entry = new("e1", "Hello", 0, translatedText: "Bonjour");
        TranslationEntryViewModel viewModel = new(entry, new UndoRedoStack());

        Assert.Null(viewModel.CharacterTablePreviewText);
    }

    [Fact]
    public void CharacterTablePreviewText_round_trips_a_representable_translation()
    {
        TranslationEntry entry = new("e1", "Hello", 0, translatedText: "Bonjour");
        TranslationEntryViewModel viewModel = new(entry, new UndoRedoStack(), characterTable: new StrictAsciiCharacterTable());

        Assert.Equal("Bonjour", viewModel.CharacterTablePreviewText);
    }

    [Fact]
    public void CharacterTablePreviewText_reports_an_unsupported_character_instead_of_throwing()
    {
        TranslationEntry entry = new("e1", "Hello", 0, translatedText: "Café");
        TranslationEntryViewModel viewModel = new(entry, new UndoRedoStack(), characterTable: new StrictAsciiCharacterTable());

        Assert.NotNull(viewModel.CharacterTablePreviewText);
        Assert.NotEqual("Café", viewModel.CharacterTablePreviewText);
    }

    [Fact]
    public void RefreshFromEntry_notifies_a_change_made_directly_on_the_entry()
    {
        TranslationEntry entry = new("e1", "Hello", 0);
        TranslationEntryViewModel viewModel = new(entry, new UndoRedoStack());
        int notifications = 0;
        viewModel.PropertyChanged += (_, _) => notifications++;

        entry.TranslatedText = "Bonjour";
        viewModel.RefreshFromEntry();

        Assert.True(notifications > 0);
        Assert.Equal("Bonjour", viewModel.TranslatedText);
    }
}
