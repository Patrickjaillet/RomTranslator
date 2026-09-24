// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.Windows;
using Microsoft.Win32;
using RomTranslator.App.ViewModels;
using RomTranslator.Core.Localization;

namespace RomTranslator.App.Views;

/// <summary>Comparaison de deux images ROM (originale et modifiée) et génération de patch IPS.</summary>
public partial class BinaryDiffWindow : Wpf.Ui.Controls.FluentWindow
{
    private readonly BinaryDiffViewModel _viewModel;

    /// <summary>Initialise la fenêtre de comparaison.</summary>
    public BinaryDiffWindow(BinaryDiffViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    private void OnCreateIpsPatchClick(object sender, RoutedEventArgs e)
    {
        SaveFileDialog dialog = new()
        {
            Filter = "IPS (*.ips)|*.ips",
            FileName = "patch.ips",
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            _viewModel.CreateIpsPatch(dialog.FileName);
            MessageBox.Show(
                this,
                string.Format(CultureInfo.CurrentCulture, Strings.Diff_IpsPatchCreated, dialog.FileName),
                Title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (NotSupportedException)
        {
            MessageBox.Show(this, Strings.Diff_IpsLimitExceeded, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
