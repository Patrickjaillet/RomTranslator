// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.ComponentModel;
using System.Windows;
using RomTranslator.App.Services;
using RomTranslator.App.ViewModels;
using RomTranslator.Core.Configuration;
using RomTranslator.Core.Localization;
using RomTranslator.Core.Portability;
using RomTranslator.Modules.SegaSaturn.CharacterTables;
using RomTranslator.Modules.SegaSaturn.Views;

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

    /// <summary>
    /// Ouvre l'assistant de création d'un projet Saturn, modal par rapport à la fenêtre principale.
    /// </summary>
    /// <returns>Le chemin du projet créé si l'utilisateur a choisi de l'ouvrir immédiatement, sinon <see langword="null" />.</returns>
    internal static string? ShowNewSaturnProjectWizard(Window owner, NewSaturnProjectViewModel wizardViewModel)
    {
        NewSaturnProjectWindow window = new(wizardViewModel) { Owner = owner };
        return window.ShowDialog() == true ? window.CreatedProjectPath : null;
    }

    /// <summary>Demande à l'utilisateur un fichier de projet <c>.rtproj</c> à ouvrir.</summary>
    /// <returns>Le chemin choisi, ou <see langword="null" /> si l'utilisateur a annulé.</returns>
    internal static string? PromptOpenProjectPath(Window owner)
    {
        Microsoft.Win32.OpenFileDialog dialog = new() { Filter = Strings.OpenProject_FileFilter };
        return dialog.ShowDialog(owner) == true ? dialog.FileName : null;
    }

    /// <summary>Ouvre l'éditeur de table de caractères du module Saturn, modal par rapport à la fenêtre principale.</summary>
    internal static void ShowCharacterTableEditor(Window owner, CharacterTableEditorViewModel viewModel)
    {
        CharacterTableEditorWindow window = new(viewModel) { Owner = owner };
        window.ShowDialog();
    }

    /// <summary>Demande à l'utilisateur le chemin de l'image ROM traduite (fichier <c>.cue</c>) à générer.</summary>
    /// <returns>Le chemin choisi, ou <see langword="null" /> si l'utilisateur a annulé.</returns>
    internal static string? PromptSaveTranslatedRomPath(Window owner)
    {
        Microsoft.Win32.SaveFileDialog dialog = new() { Filter = Strings.NewProject_ImageFileFilter, FileName = "traduit.cue" };
        return dialog.ShowDialog(owner) == true ? dialog.FileName : null;
    }

    /// <summary>Demande à l'utilisateur le chemin du patch IPS à générer.</summary>
    /// <returns>Le chemin choisi, ou <see langword="null" /> si l'utilisateur a annulé.</returns>
    internal static string? PromptSaveIpsPatchPath(Window owner)
    {
        Microsoft.Win32.SaveFileDialog dialog = new() { Filter = Strings.Saturn_Project_IpsFileFilter, FileName = "patch.ips" };
        return dialog.ShowDialog(owner) == true ? dialog.FileName : null;
    }
}
