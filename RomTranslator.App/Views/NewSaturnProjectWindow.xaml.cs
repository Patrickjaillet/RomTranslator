// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Windows;
using Microsoft.Win32;
using RomTranslator.App.ViewModels;
using RomTranslator.Core.Localization;

namespace RomTranslator.App.Views;

/// <summary>Assistant de création d'un projet de traduction Sega Saturn.</summary>
public partial class NewSaturnProjectWindow : Wpf.Ui.Controls.FluentWindow
{
    private readonly NewSaturnProjectViewModel _viewModel;

    /// <summary>Initialise l'assistant.</summary>
    public NewSaturnProjectWindow(NewSaturnProjectViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    /// <summary>Chemin du projet créé, disponible après une fermeture par « Ouvrir le projet ».</summary>
    public string? CreatedProjectPath { get; private set; }

    private void OnBrowseImageClick(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new() { Filter = Strings.NewProject_ImageFileFilter };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.ValidateImageCommand.Execute(dialog.FileName);
        }
    }

    private void OnBrowseCharacterTableClick(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new() { Filter = Strings.Saturn_CharacterTableEditor_FileFilter };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.ExternalCharacterTablePath = dialog.FileName;
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnOpenProjectClick(object sender, RoutedEventArgs e)
    {
        CreatedProjectPath = _viewModel.CreatedProjectPath;
        DialogResult = true;
        Close();
    }
}
