// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.Input;
using RomTranslator.App.Services;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Binary;
using RomTranslator.Core.Configuration;
using RomTranslator.Core.Information;
using RomTranslator.Core.Localization;
using RomTranslator.Core.Projects;
using RomTranslator.Modules.SegaSaturn;
using RomTranslator.Modules.SegaSaturn.CharacterTables;
using RomTranslator.Modules.SegaSaturn.Disc;
using RomTranslator.Modules.SegaSaturn.ViewModels;

namespace RomTranslator.App.ViewModels;

/// <summary>Modèle de vue de la fenêtre principale : onglets, menus, barre de commandes et barre de statut.</summary>
public sealed class MainViewModel
{
    /// <summary>Identifiant de l'onglet d'accueil.</summary>
    public const string HomeTabId = "home";

    private static readonly CompositeFormat LinkOpenFailedFormat = CompositeFormat.Parse(Strings.StatusBar_LinkOpenFailed);
    private static readonly CompositeFormat ProjectOpenFailedFormat = CompositeFormat.Parse(Strings.StatusBar_ProjectOpenFailed);
    private static readonly CompositeFormat RomExportedFormat = CompositeFormat.Parse(Strings.StatusBar_RomExported);
    private static readonly CompositeFormat RomExportedWithMessagesFormat = CompositeFormat.Parse(Strings.StatusBar_RomExportedWithMessages);
    private static readonly CompositeFormat RomExportFailedFormat = CompositeFormat.Parse(Strings.StatusBar_RomExportFailed);
    private static readonly CompositeFormat PatchExportedFormat = CompositeFormat.Parse(Strings.StatusBar_PatchExported);
    private static readonly CompositeFormat PatchExportFailedFormat = CompositeFormat.Parse(Strings.StatusBar_PatchExportFailed);
    private static readonly CompositeFormat ProjectSavedFormat = CompositeFormat.Parse(Strings.StatusBar_ProjectSaved);
    private static readonly CompositeFormat ProjectSaveFailedFormat = CompositeFormat.Parse(Strings.StatusBar_ProjectSaveFailed);

    private readonly SettingsStore _settingsStore;
    private readonly AppSettings _settings;
    private readonly TranslationProjectStore _projectStore = new();
    private readonly ConsoleModuleRegistry _consoleModules = new();
    private readonly Func<NewSaturnProjectViewModel, string?> _openNewSaturnProjectWizard;
    private readonly Func<string?> _promptOpenProjectPath;
    private readonly Action<CharacterTableEditorViewModel> _openCharacterTableEditor;
    private readonly Func<string?> _promptSaveTranslatedRomPath;
    private readonly Func<string?> _promptSaveIpsPatchPath;
    private readonly string _tempDirectory;
    private readonly RelayCommand _saveProjectCommand;

    /// <summary>Initialise la fenêtre principale.</summary>
    /// <param name="info">Identité de l'application.</param>
    /// <param name="settingsStore">Magasin des paramètres.</param>
    /// <param name="settings">Paramètres chargés au démarrage.</param>
    /// <param name="urlLauncher">Service d'ouverture des adresses externes.</param>
    /// <param name="exit">Action qui ferme l'application.</param>
    /// <param name="languageCode">Code court de la langue active.</param>
    /// <param name="applicationDirectory">Dossier de l'application, pour les modules consoles.</param>
    /// <param name="projectsDirectory">Dossier portable des projets de traduction.</param>
    /// <param name="tempDirectory">Dossier portable de fichiers temporaires (jamais le dossier temporaire système).</param>
    /// <param name="openSettings">Action qui ouvre l'écran de paramètres.</param>
    /// <param name="openAbout">Action qui ouvre la boîte « À propos ».</param>
    /// <param name="openNewSaturnProjectWizard">
    /// Ouvre l'assistant de création d'un projet Saturn et retourne le chemin du projet créé si l'utilisateur
    /// a choisi de l'ouvrir immédiatement, ou <see langword="null" /> si l'assistant a été annulé.
    /// </param>
    /// <param name="promptOpenProjectPath">Demande à l'utilisateur un fichier de projet à ouvrir, ou <see langword="null" /> si annulé.</param>
    /// <param name="openCharacterTableEditor">Ouvre l'éditeur de table de caractères du module Saturn.</param>
    /// <param name="promptSaveTranslatedRomPath">Demande à l'utilisateur le chemin de l'image ROM traduite à générer, ou <see langword="null" /> si annulé.</param>
    /// <param name="promptSaveIpsPatchPath">Demande à l'utilisateur le chemin du patch IPS à générer, ou <see langword="null" /> si annulé.</param>
    public MainViewModel(
        ApplicationInfo info,
        SettingsStore settingsStore,
        AppSettings settings,
        IUrlLauncher urlLauncher,
        Action exit,
        string languageCode,
        string applicationDirectory,
        string projectsDirectory,
        string tempDirectory,
        Action openSettings,
        Action openAbout,
        Func<NewSaturnProjectViewModel, string?> openNewSaturnProjectWizard,
        Func<string?> promptOpenProjectPath,
        Action<CharacterTableEditorViewModel> openCharacterTableEditor,
        Func<string?> promptSaveTranslatedRomPath,
        Func<string?> promptSaveIpsPatchPath)
    {
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(settingsStore);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(urlLauncher);
        ArgumentNullException.ThrowIfNull(exit);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectsDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(tempDirectory);
        ArgumentNullException.ThrowIfNull(openSettings);
        ArgumentNullException.ThrowIfNull(openAbout);
        ArgumentNullException.ThrowIfNull(openNewSaturnProjectWizard);
        ArgumentNullException.ThrowIfNull(promptOpenProjectPath);
        ArgumentNullException.ThrowIfNull(openCharacterTableEditor);
        ArgumentNullException.ThrowIfNull(promptSaveTranslatedRomPath);
        ArgumentNullException.ThrowIfNull(promptSaveIpsPatchPath);

        Info = info;
        _settingsStore = settingsStore;
        _settings = settings;
        _openNewSaturnProjectWizard = openNewSaturnProjectWizard;
        _promptOpenProjectPath = promptOpenProjectPath;
        _openCharacterTableEditor = openCharacterTableEditor;
        _promptSaveTranslatedRomPath = promptSaveTranslatedRomPath;
        _promptSaveIpsPatchPath = promptSaveIpsPatchPath;

        SaturnModule = new SaturnConsoleModule(applicationDirectory, _projectStore);
        _consoleModules.Register(SaturnModule);
        ProjectsDirectory = projectsDirectory;
        _tempDirectory = tempDirectory;

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

        NewProject = new AppCommandViewModel(
            Strings.Menu_File_NewProject, Strings.Menu_File_NewProject_ToolTip, IconKeys.NewProject, new RelayCommand(CreateNewSaturnProject), "Ctrl+N");
        OpenProject = new AppCommandViewModel(
            Strings.Menu_File_OpenProject, Strings.Menu_File_OpenProject_ToolTip, IconKeys.Open, new RelayCommand(OpenExistingProject), "Ctrl+O");
        _saveProjectCommand = new RelayCommand(SaveActiveProject, CanSaveActiveProject);
        SaveProject = new AppCommandViewModel(
            Strings.Menu_File_SaveProject, Strings.Menu_File_SaveProject_ToolTip, IconKeys.Save, _saveProjectCommand, "Ctrl+S");

        Tabs.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(TabsViewModel.SelectedItem))
            {
                _saveProjectCommand.NotifyCanExecuteChanged();
            }
        };
        ExportProject = Unavailable(Strings.Menu_File_ExportProject, Strings.Menu_File_ExportProject_ToolTip, IconKeys.Export, "Ctrl+E");
        Exit = new AppCommandViewModel(Strings.Menu_File_Exit, Strings.Menu_File_Exit_ToolTip, IconKeys.Exit, new RelayCommand(exit), "Alt+F4");

        Undo = Unavailable(Strings.Menu_Edit_Undo, Strings.Menu_Edit_Undo_ToolTip, IconKeys.Undo);
        Redo = Unavailable(Strings.Menu_Edit_Redo, Strings.Menu_Edit_Redo_ToolTip, IconKeys.Redo);

        Settings = new AppCommandViewModel(
            Strings.Menu_Tools_Settings, Strings.Menu_Tools_Settings_ToolTip, IconKeys.Settings, new RelayCommand(openSettings));

        Help = new AppCommandViewModel(Strings.Menu_Help_Online, Strings.Menu_Help_Online_ToolTip, IconKeys.Help, Website.Command);
        About = new AppCommandViewModel(
            Strings.Menu_Help_About, Strings.Menu_Help_About_ToolTip, IconKeys.About, new RelayCommand(openAbout));

        CommandBarPrimary = new[] { NewProject, OpenProject, SaveProject, ExportProject };
        CommandBarSecondary = new[] { Settings, Help };

        Status.ReportMessage(Strings.StatusBar_Ready);
    }

    /// <summary>Identité de l'application.</summary>
    public ApplicationInfo Info { get; }

    /// <summary>Titre affiché dans la barre de titre.</summary>
    public string Title => Info.Name;

    /// <summary>Module console Sega Saturn, seul module embarqué pour l'instant.</summary>
    public SaturnConsoleModule SaturnModule { get; }

    /// <summary>Dossier portable des projets de traduction, proposé par défaut à la création d'un projet.</summary>
    public string ProjectsDirectory { get; }

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
        Status.ReportMessage(string.Format(CultureInfo.CurrentCulture, LinkOpenFailedFormat, url));
    }

    private void CreateNewSaturnProject()
    {
        NewSaturnProjectViewModel wizard = new(SaturnModule, _projectStore, AppContext.BaseDirectory, ProjectsDirectory);
        string? createdProjectPath = _openNewSaturnProjectWizard(wizard);

        if (createdProjectPath is not null)
        {
            OpenProjectTab(createdProjectPath);
        }
    }

    private void OpenExistingProject()
    {
        string? path = _promptOpenProjectPath();
        if (path is not null)
        {
            OpenProjectTab(path);
        }
    }

    private void OpenProjectTab(string projectPath)
    {
        string tabId = Path.GetFullPath(projectPath);

        if (Tabs.SelectTab(tabId))
        {
            return;
        }

        try
        {
            ConsoleProjectContext context = SaturnModule.LoadProjectContext(projectPath);
            TranslationEditorViewModel editor = new(context.Project, context.LengthPolicy, context.CharacterTable);
            SaturnRomInfoViewModel? romInfo = TryLoadRomInfo(context.Project.RomPath);

            SaturnProjectViewModel projectTab = new(
                romInfo,
                editor,
                openCharacterTableEditor: () => OpenCharacterTableEditorFor(context.CharacterTable),
                exportTranslatedRom: () => ExportTranslatedRom(context.Project, context.CharacterTable),
                exportPatch: () => ExportPatch(context.Project, context.CharacterTable));

            Tabs.AddTab(new TabItemViewModel(tabId, context.Project.Name, SaturnModule.IconKey, projectTab));
            Tabs.SelectTab(tabId);
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            Status.ReportMessage(string.Format(CultureInfo.CurrentCulture, ProjectOpenFailedFormat, exception.Message));
        }
    }

    private SaturnRomInfoViewModel? TryLoadRomInfo(string romPath)
    {
        try
        {
            return new SaturnRomInfoViewModel(SaturnModule.RomLoader.Load(romPath));
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException)
        {
            return null;
        }
    }

    private void OpenCharacterTableEditorFor(ICharacterTable characterTable)
    {
        CharacterTableFile? file = characterTable is SaturnCharacterTable saturnTable ? saturnTable.TableFile : null;
        _openCharacterTableEditor(new CharacterTableEditorViewModel(file));
    }

    private void ExportTranslatedRom(TranslationProject project, ICharacterTable characterTable)
    {
        string? outputPath = _promptSaveTranslatedRomPath();
        if (outputPath is null)
        {
            return;
        }

        try
        {
            TextInjectionResult result = SaturnModule.TextInjector.Inject(project.RomPath, outputPath, project.Entries.ConvertAll(entry => (ITranslationEntry)entry), characterTable);
            Status.ReportMessage(result.Messages.Count == 0
                ? string.Format(CultureInfo.CurrentCulture, RomExportedFormat, outputPath)
                : string.Format(CultureInfo.CurrentCulture, RomExportedWithMessagesFormat, outputPath, result.Messages.Count));
        }
        catch (Exception exception) when (exception is IOException or InvalidOperationException)
        {
            Status.ReportMessage(string.Format(CultureInfo.CurrentCulture, RomExportFailedFormat, exception.Message));
        }
    }

    private void ExportPatch(TranslationProject project, ICharacterTable characterTable)
    {
        string? patchPath = _promptSaveIpsPatchPath();
        if (patchPath is null)
        {
            return;
        }

        string temporaryDirectory = Path.Combine(_tempDirectory, Path.GetRandomFileName());
        Directory.CreateDirectory(temporaryDirectory);
        string temporaryRomPath = Path.Combine(temporaryDirectory, "translated.cue");

        try
        {
            SaturnModule.TextInjector.Inject(project.RomPath, temporaryRomPath, project.Entries.ConvertAll(entry => (ITranslationEntry)entry), characterTable);

            string sourceDataFile = CueSheetReader.Read(project.RomPath).FirstDataTrack!.DataFilePath;
            string translatedDataFile = CueSheetReader.Read(temporaryRomPath).FirstDataTrack!.DataFilePath;
            IpsPatch.Create(sourceDataFile, translatedDataFile, patchPath);

            Status.ReportMessage(string.Format(CultureInfo.CurrentCulture, PatchExportedFormat, patchPath));
        }
        catch (Exception exception) when (exception is IOException or InvalidOperationException or NotSupportedException or InvalidDataException)
        {
            Status.ReportMessage(string.Format(CultureInfo.CurrentCulture, PatchExportFailedFormat, exception.Message));
        }
        finally
        {
            TryDeleteDirectory(temporaryDirectory);
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (IOException)
        {
            // Le nettoyage des fichiers temporaires n'est pas critique : ils seront balayés par le système.
        }
    }

    private bool CanSaveActiveProject()
    {
        return Tabs.SelectedItem?.Content is SaturnProjectViewModel;
    }

    private void SaveActiveProject()
    {
        if (Tabs.SelectedItem?.Content is not SaturnProjectViewModel { Editor: var editor })
        {
            return;
        }

        try
        {
            _projectStore.Save(editor.Project, Tabs.SelectedItem.Id);
            Status.ReportMessage(string.Format(CultureInfo.CurrentCulture, ProjectSavedFormat, editor.Project.Name));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Status.ReportMessage(string.Format(CultureInfo.CurrentCulture, ProjectSaveFailedFormat, exception.Message));
        }
    }
}
