// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.IO;
using RomTranslator.Modules.SegaSaturn.CharacterTables;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.CharacterTables;

public sealed class CharacterTableEditorViewModelTests
{
    [Fact]
    public void Constructor_populates_entries_from_a_loaded_table()
    {
        CharacterTableFile file = CharacterTableFileReader.Parse(new[] { "41=A", "42=B" });

        CharacterTableEditorViewModel editor = new(file);

        Assert.Equal(2, editor.Entries.Count);
        Assert.True(editor.IsValid);
    }

    [Fact]
    public void Constructor_without_a_table_starts_empty()
    {
        CharacterTableEditorViewModel editor = new();

        Assert.Empty(editor.Entries);
        Assert.True(editor.IsValid);
    }

    [Fact]
    public void AddEntryCommand_adds_and_selects_a_new_empty_entry()
    {
        CharacterTableEditorViewModel editor = new();

        editor.AddEntryCommand.Execute(null);

        Assert.Single(editor.Entries);
        Assert.Same(editor.Entries[0], editor.SelectedEntry);
    }

    [Fact]
    public void RemoveEntryCommand_is_disabled_without_a_selection()
    {
        CharacterTableEditorViewModel editor = new();

        Assert.False(editor.RemoveEntryCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveEntryCommand_removes_the_selected_entry()
    {
        CharacterTableEditorViewModel editor = new();
        editor.AddEntryCommand.Execute(null);

        Assert.True(editor.RemoveEntryCommand.CanExecute(null));
        editor.RemoveEntryCommand.Execute(null);

        Assert.Empty(editor.Entries);
        Assert.Null(editor.SelectedEntry);
    }

    [Fact]
    public void ValidationIssues_detects_a_duplicate_byte_sequence_live()
    {
        CharacterTableFile file = CharacterTableFileReader.Parse(new[] { "41=A" });
        CharacterTableEditorViewModel editor = new(file);

        editor.AddEntryCommand.Execute(null);
        editor.SelectedEntry!.HexBytes = "41";
        editor.SelectedEntry!.Text = "B";

        Assert.False(editor.IsValid);
        Assert.NotEmpty(editor.ValidationIssues);
    }

    [Fact]
    public void ValidationIssues_ignores_entries_still_being_typed()
    {
        CharacterTableEditorViewModel editor = new();

        editor.AddEntryCommand.Execute(null);
        editor.SelectedEntry!.HexBytes = "4";

        Assert.True(editor.IsValid);
    }

    [Fact]
    public void Save_then_Load_round_trips_the_edited_entries()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "edited.tbl");
        CharacterTableEditorViewModel editor = new();
        editor.AddEntryCommand.Execute(null);
        editor.SelectedEntry!.HexBytes = "41";
        editor.SelectedEntry!.Text = "A";

        editor.Save(path);

        CharacterTableEditorViewModel reloaded = new();
        reloaded.Load(path);

        CharacterTableEntryViewModel entry = Assert.Single(reloaded.Entries);
        Assert.Equal("41", entry.HexBytes);
        Assert.Equal("A", entry.Text);
    }

    [Fact]
    public void Load_replaces_the_current_entries()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "other.tbl");
        File.WriteAllText(path, "42=B\n");

        CharacterTableFile initial = CharacterTableFileReader.Parse(new[] { "41=A" });
        CharacterTableEditorViewModel editor = new(initial);

        editor.Load(path);

        CharacterTableEntryViewModel entry = Assert.Single(editor.Entries);
        Assert.Equal("B", entry.Text);
    }
}
