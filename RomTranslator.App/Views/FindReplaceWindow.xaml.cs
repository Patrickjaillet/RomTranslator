// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Windows;
using RomTranslator.App.ViewModels;

namespace RomTranslator.App.Views;

/// <summary>Fenêtre de recherche et remplacement dans les traductions du projet.</summary>
public partial class FindReplaceWindow : Wpf.Ui.Controls.FluentWindow
{
    /// <summary>Initialise la fenêtre de recherche/remplacement.</summary>
    public FindReplaceWindow(FindReplaceViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        DataContext = viewModel;
        InitializeComponent();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
