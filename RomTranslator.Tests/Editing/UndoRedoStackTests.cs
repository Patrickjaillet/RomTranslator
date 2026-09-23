// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.Core.Editing;
using Xunit;

namespace RomTranslator.Tests.Editing;

public sealed class UndoRedoStackTests
{
    private sealed class RecordingCommand : IEditCommand
    {
        public int DoCount { get; private set; }
        public int UndoCount { get; private set; }

        public void Do() => DoCount++;
        public void Undo() => UndoCount++;
    }

    [Fact]
    public void Execute_runs_the_command_and_enables_undo()
    {
        UndoRedoStack stack = new();
        RecordingCommand command = new();

        stack.Execute(command);

        Assert.Equal(1, command.DoCount);
        Assert.True(stack.CanUndo);
        Assert.False(stack.CanRedo);
    }

    [Fact]
    public void Undo_reverts_the_last_command_and_enables_redo()
    {
        UndoRedoStack stack = new();
        RecordingCommand command = new();
        stack.Execute(command);

        bool undone = stack.Undo();

        Assert.True(undone);
        Assert.Equal(1, command.UndoCount);
        Assert.False(stack.CanUndo);
        Assert.True(stack.CanRedo);
    }

    [Fact]
    public void Redo_reapplies_the_undone_command()
    {
        UndoRedoStack stack = new();
        RecordingCommand command = new();
        stack.Execute(command);
        stack.Undo();

        bool redone = stack.Redo();

        Assert.True(redone);
        Assert.Equal(2, command.DoCount);
        Assert.True(stack.CanUndo);
        Assert.False(stack.CanRedo);
    }

    [Fact]
    public void Undo_returns_false_when_there_is_nothing_to_undo()
    {
        UndoRedoStack stack = new();

        Assert.False(stack.Undo());
    }

    [Fact]
    public void Redo_returns_false_when_there_is_nothing_to_redo()
    {
        UndoRedoStack stack = new();

        Assert.False(stack.Redo());
    }

    [Fact]
    public void Executing_a_new_command_clears_the_redo_stack()
    {
        UndoRedoStack stack = new();
        stack.Execute(new RecordingCommand());
        stack.Undo();

        stack.Execute(new RecordingCommand());

        Assert.False(stack.CanRedo);
    }

    [Fact]
    public void Clear_empties_both_stacks()
    {
        UndoRedoStack stack = new();
        stack.Execute(new RecordingCommand());

        stack.Clear();

        Assert.False(stack.CanUndo);
        Assert.False(stack.CanRedo);
    }

    [Fact]
    public void StateChanged_fires_on_execute_undo_and_redo()
    {
        UndoRedoStack stack = new();
        int notifications = 0;
        stack.StateChanged += (_, _) => notifications++;

        stack.Execute(new RecordingCommand());
        stack.Undo();
        stack.Redo();

        Assert.Equal(3, notifications);
    }

    [Fact]
    public void Execute_rejects_null()
    {
        UndoRedoStack stack = new();

        Assert.Throws<ArgumentNullException>(() => stack.Execute(null!));
    }
}
