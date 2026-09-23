// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Windows.Input;

namespace RomTranslator.App.ViewModels;

/// <summary>
/// Commande d'une fonctionnalité pas encore livrée : toujours désactivée. Les menus et boutons associés
/// s'affichent grisés jusqu'à l'arrivée de la fonctionnalité.
/// </summary>
public sealed class UnavailableCommand : ICommand
{
    /// <summary>Instance unique.</summary>
    public static UnavailableCommand Instance { get; } = new();

    private UnavailableCommand()
    {
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        return false;
    }

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        throw new InvalidOperationException("Cette fonctionnalité n'est pas encore disponible.");
    }
}
