// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomTranslator.App.Services;
using RomTranslator.Core.Configuration;
using RomTranslator.Core.Localization;
using RomTranslator.Core.Portability;

namespace RomTranslator.App.ViewModels;

/// <summary>
/// Modèle de vue de l'écran de paramètres généraux : langue, thème (verrouillé sur clair, affiché en lecture
/// seule) et chemins des dossiers portables. Les modifications ne sont écrites dans le fichier de paramètres
/// qu'à l'enregistrement explicite (<see cref="SaveCommand" />).
/// </summary>
public sealed class SettingsViewModel : ObservableObject
{
    private static readonly CompositeFormat LinkOpenFailedFormat = CompositeFormat.Parse(Strings.StatusBar_LinkOpenFailed);

    private readonly SettingsStore _settingsStore;
    private readonly AppSettings _settings;
    private readonly IFolderLauncher _folderLauncher;
    private readonly Action<string> _reportMessage;

    private LanguageOptionViewModel _selectedLanguage;
    private bool _isSaved;

    /// <summary>Initialise l'écran de paramètres.</summary>
    /// <param name="settingsStore">Magasin des paramètres.</param>
    /// <param name="settings">Paramètres chargés au démarrage.</param>
    /// <param name="locations">Emplacements portables affichés (configuration, projets, journaux).</param>
    /// <param name="folderLauncher">Service d'ouverture d'un dossier dans l'explorateur.</param>
    /// <param name="reportMessage">Appelée avec un message à afficher dans la barre de statut.</param>
    public SettingsViewModel(
        SettingsStore settingsStore,
        AppSettings settings,
        PortableLocations locations,
        IFolderLauncher folderLauncher,
        Action<string> reportMessage)
    {
        ArgumentNullException.ThrowIfNull(settingsStore);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(locations);
        ArgumentNullException.ThrowIfNull(folderLauncher);
        ArgumentNullException.ThrowIfNull(reportMessage);

        _settingsStore = settingsStore;
        _settings = settings;
        _folderLauncher = folderLauncher;
        _reportMessage = reportMessage;

        ConfigDirectory = locations.ConfigDirectory;
        ProjectsDirectory = locations.ProjectsDirectory;
        LogsDirectory = locations.LogsDirectory;

        LanguageOptions = new List<LanguageOptionViewModel>
        {
            new(null, Strings.Settings_LanguageSystem),
            new("fr", Strings.Settings_LanguageFrench),
            new("en", Strings.Settings_LanguageEnglish),
        };

        _selectedLanguage = LanguageOptions.FirstOrDefault(option => option.LanguageCode == settings.LanguageCode)
            ?? LanguageOptions[0];

        OpenConfigFolderCommand = new RelayCommand(() => OpenFolder(ConfigDirectory));
        OpenProjectsFolderCommand = new RelayCommand(() => OpenFolder(ProjectsDirectory));
        OpenLogsFolderCommand = new RelayCommand(() => OpenFolder(LogsDirectory));
        SaveCommand = new RelayCommand(Save);
    }

    /// <summary>Choix de langue proposés (suivre le système, français, anglais).</summary>
    public IReadOnlyList<LanguageOptionViewModel> LanguageOptions { get; }

    /// <summary>Choix de langue actuellement sélectionné.</summary>
    public LanguageOptionViewModel SelectedLanguage
    {
        get => _selectedLanguage;
        set => SetProperty(ref _selectedLanguage, value);
    }

    /// <summary>Dossier de configuration.</summary>
    public string ConfigDirectory { get; }

    /// <summary>Dossier des projets de traduction.</summary>
    public string ProjectsDirectory { get; }

    /// <summary>Dossier des journaux.</summary>
    public string LogsDirectory { get; }

    /// <summary>Indique si les paramètres ont été enregistrés au moins une fois depuis l'ouverture de l'écran.</summary>
    public bool IsSaved
    {
        get => _isSaved;
        private set => SetProperty(ref _isSaved, value);
    }

    /// <summary>Ouvre le dossier de configuration dans l'explorateur.</summary>
    public RelayCommand OpenConfigFolderCommand { get; }

    /// <summary>Ouvre le dossier des projets dans l'explorateur.</summary>
    public RelayCommand OpenProjectsFolderCommand { get; }

    /// <summary>Ouvre le dossier des journaux dans l'explorateur.</summary>
    public RelayCommand OpenLogsFolderCommand { get; }

    /// <summary>Enregistre les paramètres modifiés.</summary>
    public RelayCommand SaveCommand { get; }

    private void OpenFolder(string path)
    {
        if (!_folderLauncher.TryOpen(path))
        {
            _reportMessage(string.Format(CultureInfo.CurrentCulture, LinkOpenFailedFormat, path));
        }
    }

    private void Save()
    {
        _settings.LanguageCode = SelectedLanguage.LanguageCode;

        try
        {
            _settingsStore.Save(_settings);
            IsSaved = true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            IsSaved = false;
        }
    }
}
