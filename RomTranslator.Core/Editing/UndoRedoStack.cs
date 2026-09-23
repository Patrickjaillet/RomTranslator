// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;

namespace RomTranslator.Core.Editing;

/// <summary>
/// Pile d'annulation/rétablissement générique. Exécuter une nouvelle commande vide la pile de rétablissement
/// (un nouvel embranchement de l'historique remplace celui qui pouvait être rétabli).
/// </summary>
public sealed class UndoRedoStack
{
    private readonly Stack<IEditCommand> _undoStack = new();
    private readonly Stack<IEditCommand> _redoStack = new();

    /// <summary>Indique si une annulation est possible.</summary>
    public bool CanUndo => _undoStack.Count > 0;

    /// <summary>Indique si un rétablissement est possible.</summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>Notifié après chaque exécution, annulation ou rétablissement (mise à jour de l'état <c>CanUndo</c>/<c>CanRedo</c>).</summary>
    public event EventHandler? StateChanged;

    /// <summary>Exécute une commande et l'empile pour une annulation ultérieure.</summary>
    public void Execute(IEditCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        command.Do();
        _undoStack.Push(command);
        _redoStack.Clear();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Annule la dernière commande exécutée.</summary>
    /// <returns><see langword="true" /> si une commande a été annulée.</returns>
    public bool Undo()
    {
        if (_undoStack.Count == 0)
        {
            return false;
        }

        IEditCommand command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
        StateChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>Rétablit la dernière commande annulée.</summary>
    /// <returns><see langword="true" /> si une commande a été rétablie.</returns>
    public bool Redo()
    {
        if (_redoStack.Count == 0)
        {
            return false;
        }

        IEditCommand command = _redoStack.Pop();
        command.Do();
        _undoStack.Push(command);
        StateChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>Vide l'historique (par exemple à l'ouverture d'un nouveau projet).</summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
