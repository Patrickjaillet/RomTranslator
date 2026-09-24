// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Windows.Input;
using RomTranslator.App.ViewModels;

namespace RomTranslator.App.Views;

/// <summary>Visualiseur hexadécimal en lecture seule d'une image ROM.</summary>
public partial class HexViewerWindow : Wpf.Ui.Controls.FluentWindow
{
    /// <summary>Initialise le visualiseur.</summary>
    public HexViewerWindow(HexViewerViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        DataContext = viewModel;
        InitializeComponent();
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}
