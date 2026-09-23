// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.ComponentModel;
using System.Windows;
using RomTranslator.App.Services;
using RomTranslator.App.ViewModels;
using RomTranslator.Core.Configuration;
using RomTranslator.Core.Portability;

namespace RomTranslator.App.Views;

/// <summary>Fenêtre principale : restaure et enregistre sa position, sa taille et son onglet actif.</summary>
public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel, WindowSettings savedWindow)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(savedWindow);

        _viewModel = viewModel;
        DataContext = viewModel;

        InitializeComponent();
        RestorePlacement(savedWindow);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (!e.Cancel)
        {
            _viewModel.SaveState(CaptureWindowSettings());
        }
    }

    private void RestorePlacement(WindowSettings saved)
    {
        WindowBounds virtualScreen = new(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);

        if (WindowPlacementCalculator.Resolve(saved, virtualScreen, MinWidth, MinHeight) is { } bounds)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = bounds.Left;
            Top = bounds.Top;
            Width = bounds.Width;
            Height = bounds.Height;
        }

        if (saved.IsMaximized)
        {
            WindowState = WindowState.Maximized;
        }
    }

    private WindowSettings CaptureWindowSettings()
    {
        Rect bounds = WindowState == WindowState.Normal
            ? new Rect(Left, Top, Width, Height)
            : RestoreBounds;

        return new WindowSettings
        {
            Left = bounds.Left,
            Top = bounds.Top,
            Width = bounds.Width,
            Height = bounds.Height,
            IsMaximized = WindowState == WindowState.Maximized,
        };
    }

    /// <summary>Ouvre l'écran de paramètres, modal par rapport à la fenêtre principale.</summary>
    internal static void ShowSettings(Window owner, SettingsStore settingsStore, AppSettings settings, PortableLocations locations)
    {
        SettingsViewModel viewModel = new(
            settingsStore,
            settings,
            locations,
            new ShellFolderLauncher(),
            message => (owner as MainWindow)?._viewModel.Status.ReportMessage(message));

        SettingsWindow window = new(viewModel) { Owner = owner };
        window.ShowDialog();
    }

    /// <summary>Ouvre la boîte de dialogue « À propos », modale par rapport à la fenêtre principale.</summary>
    internal static void ShowAbout(Window owner, MainViewModel mainViewModel)
    {
        AboutViewModel viewModel = new(mainViewModel.Info, mainViewModel.Website, mainViewModel.Repository);

        AboutWindow window = new(viewModel) { Owner = owner };
        window.ShowDialog();
    }
}
