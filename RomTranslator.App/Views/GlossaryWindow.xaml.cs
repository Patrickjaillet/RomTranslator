// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Windows;
using RomTranslator.App.ViewModels;

namespace RomTranslator.App.Views;

/// <summary>Fenêtre du glossaire du projet : termes récurrents, traductions retenues et incohérences.</summary>
public partial class GlossaryWindow : Wpf.Ui.Controls.FluentWindow
{
    private readonly GlossaryViewModel _viewModel;

    /// <summary>Initialise la fenêtre du glossaire.</summary>
    public GlossaryWindow(GlossaryViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    private void OnAddClick(object sender, RoutedEventArgs e)
    {
        string sourceTerm = NewTermTextBox.Text;
        if (_viewModel.AddEntryCommand.CanExecute(sourceTerm))
        {
            _viewModel.AddEntryCommand.Execute(sourceTerm);
            NewTermTextBox.Text = string.Empty;
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
