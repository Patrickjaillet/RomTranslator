// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;

namespace RomTranslator.Core.Editing;

/// <summary>
/// Regroupe plusieurs commandes réversibles en une seule, pour qu'une opération portant sur plusieurs
/// entrées (par exemple un remplacement dans l'ensemble des traductions d'un projet) ne compte que pour une
/// seule étape d'annulation/rétablissement.
/// </summary>
public sealed class CompositeEditCommand : IEditCommand
{
    private readonly IReadOnlyList<IEditCommand> _commands;

    /// <summary>Initialise la commande composite.</summary>
    /// <param name="commands">Commandes regroupées, appliquées dans l'ordre donné et annulées dans l'ordre inverse.</param>
    public CompositeEditCommand(IReadOnlyList<IEditCommand> commands)
    {
        ArgumentNullException.ThrowIfNull(commands);

        _commands = commands;
    }

    /// <summary>Nombre de commandes regroupées.</summary>
    public int Count => _commands.Count;

    /// <inheritdoc />
    public void Do()
    {
        foreach (IEditCommand command in _commands)
        {
            command.Do();
        }
    }

    /// <inheritdoc />
    public void Undo()
    {
        for (int i = _commands.Count - 1; i >= 0; i--)
        {
            _commands[i].Undo();
        }
    }
}
