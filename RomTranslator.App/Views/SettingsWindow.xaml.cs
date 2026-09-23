// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Windows;
using RomTranslator.App.ViewModels;

namespace RomTranslator.App.Views;

/// <summary>Écran de paramètres généraux : langue, thème (verrouillé), dossiers portables.</summary>
public partial class SettingsWindow : Wpf.Ui.Controls.FluentWindow
{
    private readonly SettingsViewModel _viewModel;

    /// <summary>Initialise l'écran de paramètres.</summary>
    public SettingsWindow(SettingsViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        _viewModel.SaveCommand.Execute(null);
        if (_viewModel.IsSaved)
        {
            DialogResult = true;
            Close();
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
