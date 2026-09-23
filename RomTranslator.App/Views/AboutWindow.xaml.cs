// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.App.ViewModels;

namespace RomTranslator.App.Views;

/// <summary>Boîte de dialogue « À propos de RomTranslator ».</summary>
public partial class AboutWindow : Wpf.Ui.Controls.FluentWindow
{
    /// <summary>Initialise la boîte de dialogue.</summary>
    public AboutWindow(AboutViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        DataContext = viewModel;
        InitializeComponent();
    }
}
