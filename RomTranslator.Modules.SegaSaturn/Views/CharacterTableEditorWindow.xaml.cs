// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Windows;
using Microsoft.Win32;
using RomTranslator.Core.Localization;
using RomTranslator.Modules.SegaSaturn.CharacterTables;

namespace RomTranslator.Modules.SegaSaturn.Views;

/// <summary>Éditeur de table de caractères : associe des séquences d'octets à des caractères affichés.</summary>
public partial class CharacterTableEditorWindow : Wpf.Ui.Controls.FluentWindow
{
    private readonly CharacterTableEditorViewModel _viewModel;

    /// <summary>Initialise l'éditeur.</summary>
    /// <param name="viewModel">Modèle de vue de l'éditeur, vide ou déjà chargé.</param>
    public CharacterTableEditorWindow(CharacterTableEditorViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    private void OnLoadClick(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new() { Filter = Strings.Saturn_CharacterTableEditor_FileFilter };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.Load(dialog.FileName);
        }
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        SaveFileDialog dialog = new() { Filter = Strings.Saturn_CharacterTableEditor_FileFilter, FileName = "table.tbl" };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.Save(dialog.FileName);
        }
    }
}
