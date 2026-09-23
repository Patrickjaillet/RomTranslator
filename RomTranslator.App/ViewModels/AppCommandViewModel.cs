// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Windows.Input;

namespace RomTranslator.App.ViewModels;

/// <summary>Action de l'application présentée dans les menus et la barre de commandes.</summary>
public class AppCommandViewModel
{
    /// <summary>Initialise une action.</summary>
    /// <param name="header">Libellé (menu et bouton).</param>
    /// <param name="toolTip">Infobulle.</param>
    /// <param name="iconKey">Clé de l'icône (voir <see cref="IconKeys" />).</param>
    /// <param name="command">Commande exécutée.</param>
    /// <param name="gestureText">Raccourci clavier affiché dans les menus, ou <see langword="null" />.</param>
    public AppCommandViewModel(string header, string toolTip, string iconKey, ICommand command, string? gestureText = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(header);
        ArgumentException.ThrowIfNullOrWhiteSpace(toolTip);
        ArgumentException.ThrowIfNullOrWhiteSpace(iconKey);
        ArgumentNullException.ThrowIfNull(command);

        Header = header;
        ToolTip = toolTip;
        IconKey = iconKey;
        Command = command;
        GestureText = gestureText;
    }

    /// <summary>Libellé.</summary>
    public string Header { get; }

    /// <summary>Infobulle.</summary>
    public string ToolTip { get; }

    /// <summary>Clé de l'icône.</summary>
    public string IconKey { get; }

    /// <summary>Commande exécutée.</summary>
    public ICommand Command { get; }

    /// <summary>Raccourci clavier affiché, ou <see langword="null" />.</summary>
    public string? GestureText { get; }
}
