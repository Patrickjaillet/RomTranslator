// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.Input;
using RomTranslator.App.Services;
using RomTranslator.Core.Configuration;
using RomTranslator.Core.Information;

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
            "Site officiel", "Ouvrir le site officiel et la documentation", IconKeys.Website, ProjectLinks.WebsiteUrl, urlLauncher, ReportLinkFailure);
        Repository = new LinkItemViewModel(
            "Code source", "Consulter le code source du projet", IconKeys.Repository, ProjectLinks.RepositoryUrl, urlLauncher, ReportLinkFailure);
        Releases = new LinkItemViewModel(
            "Versions", "Télécharger la dernière version portable", IconKeys.Releases, ProjectLinks.ReleasesUrl, urlLauncher, ReportLinkFailure);
        Contact = new LinkItemViewModel(
            "Contact", "Écrire à l'auteur", IconKeys.Contact, ProjectLinks.ContactUrl, urlLauncher, ReportLinkFailure);
        Links = new[] { Website, Repository, Releases, Contact };

        Home = new HomeViewModel(info, Links);
        Tabs = new TabsViewModel(settings.Window.LastActiveTabId);
        Tabs.AddTab(new TabItemViewModel(HomeTabId, "Accueil", IconKeys.Home, Home, isPermanent: true));

        NewProject = Unavailable("Nouveau projet…", "Créer un projet de traduction (bientôt disponible)", IconKeys.NewProject, "Ctrl+N");
        OpenProject = Unavailable("Ouvrir un projet…", "Ouvrir un projet de traduction (bientôt disponible)", IconKeys.Open, "Ctrl+O");
        SaveProject = Unavailable("Enregistrer", "Enregistrer le projet (bientôt disponible)", IconKeys.Save, "Ctrl+S");
        ExportProject = Unavailable("Exporter…", "Exporter la traduction (bientôt disponible)", IconKeys.Export, "Ctrl+E");
        Exit = new AppCommandViewModel("Quitter", "Fermer RomTranslator", IconKeys.Exit, new RelayCommand(exit), "Alt+F4");

        Undo = Unavailable("Annuler", "Annuler la dernière modification (bientôt disponible)", IconKeys.Undo);
        Redo = Unavailable("Rétablir", "Rétablir la modification annulée (bientôt disponible)", IconKeys.Redo);

        Settings = Unavailable("Paramètres…", "Ouvrir les paramètres (bientôt disponible)", IconKeys.Settings);

        Help = new AppCommandViewModel("Aide en ligne", "Ouvrir l'aide en ligne", IconKeys.Help, Website.Command);
        About = Unavailable("À propos de RomTranslator", "Informations sur l'application (bientôt disponible)", IconKeys.About);

        CommandBarPrimary = new[] { NewProject, OpenProject, SaveProject, ExportProject };
        CommandBarSecondary = new[] { Settings, Help };

        Status.ReportMessage("Prêt");
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
        Status.ReportMessage("Impossible d'ouvrir : " + url);
    }
}
