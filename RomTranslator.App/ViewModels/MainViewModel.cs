// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.Input;
using RomTranslator.App.Services;
using RomTranslator.Core.Configuration;
using RomTranslator.Core.Information;
using RomTranslator.Core.Localization;

namespace RomTranslator.App.ViewModels;

/// <summary>Modèle de vue de la fenêtre principale : onglets, menus, barre de commandes et barre de statut.</summary>
public sealed class MainViewModel
{
    /// <summary>Identifiant de l'onglet d'accueil.</summary>
    public const string HomeTabId = "home";

    private readonly SettingsStore _settingsStore;
    private readonly AppSettings _settings;

    /// <summary>Initialise la fenêtre principale.</summary>
    /// <param name="info">Identité de l'application.</param>
    /// <param name="settingsStore">Magasin des paramètres.</param>
    /// <param name="settings">Paramètres chargés au démarrage.</param>
    /// <param name="urlLauncher">Service d'ouverture des adresses externes.</param>
    /// <param name="exit">Action qui ferme l'application.</param>
    /// <param name="languageCode">Code court de la langue active.</param>
    public MainViewModel(
        ApplicationInfo info,
        SettingsStore settingsStore,
        AppSettings settings,
        IUrlLauncher urlLauncher,
        Action exit,
        string languageCode)
    {
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(settingsStore);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(urlLauncher);
        ArgumentNullException.ThrowIfNull(exit);

        Info = info;
        _settingsStore = settingsStore;
        _settings = settings;

        Status = new StatusBarViewModel(languageCode);

        Website = new LinkItemViewModel(
            Strings.Link_Website, Strings.Link_Website_Description, IconKeys.Website, ProjectLinks.WebsiteUrl, urlLauncher, ReportLinkFailure);
        Repository = new LinkItemViewModel(
            Strings.Menu_Help_Repository, Strings.Link_Repository_Description, IconKeys.Repository, ProjectLinks.RepositoryUrl, urlLauncher, ReportLinkFailure);
        Releases = new LinkItemViewModel(
            Strings.Menu_Help_Releases, Strings.Link_Releases_Description, IconKeys.Releases, ProjectLinks.ReleasesUrl, urlLauncher, ReportLinkFailure);
        Contact = new LinkItemViewModel(
            Strings.Menu_Help_Contact, Strings.Link_Contact_Description, IconKeys.Contact, ProjectLinks.ContactUrl, urlLauncher, ReportLinkFailure);
        Links = new[] { Website, Repository, Releases, Contact };

        Home = new HomeViewModel(info, Links);
        Tabs = new TabsViewModel(settings.Window.LastActiveTabId);
        Tabs.AddTab(new TabItemViewModel(HomeTabId, Strings.Tab_Home, IconKeys.Home, Home, isPermanent: true));

        NewProject = Unavailable(Strings.Menu_File_NewProject, Strings.Menu_File_NewProject_ToolTip, IconKeys.NewProject, "Ctrl+N");
        OpenProject = Unavailable(Strings.Menu_File_OpenProject, Strings.Menu_File_OpenProject_ToolTip, IconKeys.Open, "Ctrl+O");
        SaveProject = Unavailable(Strings.Menu_File_SaveProject, Strings.Menu_File_SaveProject_ToolTip, IconKeys.Save, "Ctrl+S");
        ExportProject = Unavailable(Strings.Menu_File_ExportProject, Strings.Menu_File_ExportProject_ToolTip, IconKeys.Export, "Ctrl+E");
        Exit = new AppCommandViewModel(Strings.Menu_File_Exit, Strings.Menu_File_Exit_ToolTip, IconKeys.Exit, new RelayCommand(exit), "Alt+F4");

        Undo = Unavailable(Strings.Menu_Edit_Undo, Strings.Menu_Edit_Undo_ToolTip, IconKeys.Undo);
        Redo = Unavailable(Strings.Menu_Edit_Redo, Strings.Menu_Edit_Redo_ToolTip, IconKeys.Redo);

        Settings = Unavailable(Strings.Menu_Tools_Settings, Strings.Menu_Tools_Settings_ToolTip, IconKeys.Settings);

        Help = new AppCommandViewModel(Strings.Menu_Help_Online, Strings.Menu_Help_Online_ToolTip, IconKeys.Help, Website.Command);
        About = Unavailable(Strings.Menu_Help_About, Strings.Menu_Help_About_ToolTip, IconKeys.About);

        CommandBarPrimary = new[] { NewProject, OpenProject, SaveProject, ExportProject };
        CommandBarSecondary = new[] { Settings, Help };

        Status.ReportMessage(Strings.StatusBar_Ready);
    }

    /// <summary>Identité de l'application.</summary>
    public ApplicationInfo Info { get; }

    /// <summary>Titre affiché dans la barre de titre.</summary>
    public string Title => Info.Name;

    /// <summary>Onglets.</summary>
    public TabsViewModel Tabs { get; }

    /// <summary>Barre de statut.</summary>
    public StatusBarViewModel Status { get; }

    /// <summary>Écran d'accueil.</summary>
    public HomeViewModel Home { get; }

    /// <summary>Liens externes du projet.</summary>
    public IReadOnlyList<LinkItemViewModel> Links { get; }

    /// <summary>Lien vers le site officiel.</summary>
    public LinkItemViewModel Website { get; }

    /// <summary>Lien vers le code source.</summary>
    public LinkItemViewModel Repository { get; }

    /// <summary>Lien vers les versions publiées.</summary>
    public LinkItemViewModel Releases { get; }

    /// <summary>Lien de contact.</summary>
    public LinkItemViewModel Contact { get; }

    /// <summary>Menu Fichier : nouveau projet.</summary>
    public AppCommandViewModel NewProject { get; }

    /// <summary>Menu Fichier : ouvrir un projet.</summary>
    public AppCommandViewModel OpenProject { get; }

    /// <summary>Menu Fichier : enregistrer.</summary>
    public AppCommandViewModel SaveProject { get; }

    /// <summary>Menu Fichier : exporter.</summary>
    public AppCommandViewModel ExportProject { get; }

    /// <summary>Menu Fichier : quitter.</summary>
    public AppCommandViewModel Exit { get; }

    /// <summary>Menu Édition : annuler.</summary>
    public AppCommandViewModel Undo { get; }

    /// <summary>Menu Édition : rétablir.</summary>
    public AppCommandViewModel Redo { get; }

    /// <summary>Menu Outils : paramètres.</summary>
    public AppCommandViewModel Settings { get; }

    /// <summary>Menu Aide : aide en ligne.</summary>
    public AppCommandViewModel Help { get; }

    /// <summary>Menu Aide : à propos.</summary>
    public AppCommandViewModel About { get; }

    /// <summary>Boutons du groupe principal de la barre de commandes.</summary>
    public IReadOnlyList<AppCommandViewModel> CommandBarPrimary { get; }

    /// <summary>Boutons du groupe secondaire de la barre de commandes.</summary>
    public IReadOnlyList<AppCommandViewModel> CommandBarSecondary { get; }

    /// <summary>Enregistre l'état de la fenêtre et l'onglet actif dans le fichier de paramètres.</summary>
    /// <param name="window">État de la fenêtre à conserver.</param>
    /// <returns><see langword="true" /> si l'enregistrement a réussi.</returns>
    public bool SaveState(WindowSettings window)
    {
        ArgumentNullException.ThrowIfNull(window);

        window.LastActiveTabId = Tabs.SelectedItem?.Id;
        _settings.Window = window;

        try
        {
            _settingsStore.Save(_settings);
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static AppCommandViewModel Unavailable(string header, string toolTip, string iconKey, string? gestureText = null)
    {
        return new AppCommandViewModel(header, toolTip, iconKey, UnavailableCommand.Instance, gestureText);
    }

    private void ReportLinkFailure(string url)
    {
        Status.ReportMessage(string.Format(CultureInfo.CurrentCulture, Strings.StatusBar_LinkOpenFailed, url));
    }
}
