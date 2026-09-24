// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Editing;
using RomTranslator.Core.Projects;
using Xunit;

namespace RomTranslator.Tests.Editing;

public sealed class EditCommandsTests
{
    [Fact]
    public void EditTranslatedTextCommand_applies_and_reverts_the_text()
    {
        TranslationEntry entry = new("e1", "Hello", 0, translatedText: "ancien");
        EditTranslatedTextCommand command = new(entry, "nouveau");

        command.Do();
        Assert.Equal("nouveau", entry.TranslatedText);

        command.Undo();
        Assert.Equal("ancien", entry.TranslatedText);
    }

    [Fact]
    public void EditStatusCommand_applies_and_reverts_the_status()
    {
        TranslationEntry entry = new("e1", "Hello", 0, status: TranslationStatus.InProgress);
        EditStatusCommand command = new(entry, TranslationStatus.Validated);

        command.Do();
        Assert.Equal(TranslationStatus.Validated, entry.Status);

        command.Undo();
        Assert.Equal(TranslationStatus.InProgress, entry.Status);
    }

    [Fact]
    public void EditTranslatedTextCommand_integrates_with_the_undo_redo_stack()
    {
        TranslationEntry entry = new("e1", "Hello", 0);
        UndoRedoStack stack = new();

        stack.Execute(new EditTranslatedTextCommand(entry, "Bonjour"));
        Assert.Equal("Bonjour", entry.TranslatedText);

        stack.Undo();
        Assert.Equal(string.Empty, entry.TranslatedText);

        stack.Redo();
        Assert.Equal("Bonjour", entry.TranslatedText);
    }

    [Fact]
    public void CompositeEditCommand_applies_every_command_in_order()
    {
        TranslationEntry first = new("e1", "Hello", 0, translatedText: "a");
        TranslationEntry second = new("e2", "World", 8, translatedText: "b");
        CompositeEditCommand command = new(new IEditCommand[]
        {
            new EditTranslatedTextCommand(first, "aa"),
            new EditTranslatedTextCommand(second, "bb"),
        });

        command.Do();

        Assert.Equal("aa", first.TranslatedText);
        Assert.Equal("bb", second.TranslatedText);
        Assert.Equal(2, command.Count);
    }

    [Fact]
    public void CompositeEditCommand_undoes_every_command_in_reverse_order()
    {
        TranslationEntry first = new("e1", "Hello", 0, translatedText: "a");
        TranslationEntry second = new("e2", "World", 8, translatedText: "b");
        CompositeEditCommand command = new(new IEditCommand[]
        {
            new EditTranslatedTextCommand(first, "aa"),
            new EditTranslatedTextCommand(second, "bb"),
        });

        command.Do();
        command.Undo();

        Assert.Equal("a", first.TranslatedText);
        Assert.Equal("b", second.TranslatedText);
    }

    [Fact]
    public void CompositeEditCommand_integrates_with_the_undo_redo_stack_as_a_single_step()
    {
        TranslationEntry first = new("e1", "Hello", 0);
        TranslationEntry second = new("e2", "World", 8);
        UndoRedoStack stack = new();

        stack.Execute(new CompositeEditCommand(new IEditCommand[]
        {
            new EditTranslatedTextCommand(first, "Bonjour"),
            new EditTranslatedTextCommand(second, "Monde"),
        }));

        Assert.True(stack.CanUndo);
        stack.Undo();
        Assert.Equal(string.Empty, first.TranslatedText);
        Assert.Equal(string.Empty, second.TranslatedText);
        Assert.False(stack.CanUndo);
    }
}
