// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RomTranslator.App.ViewModels;
using RomTranslator.Core.Configuration;
using RomTranslator.Core.Information;
using RomTranslator.Core.Portability;
using RomTranslator.Core.Projects;
using RomTranslator.Modules.SegaSaturn.CharacterTables;
using RomTranslator.Tests.SegaSaturn.Support;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.ViewModels;

public sealed class MainViewModelTests
{
    private static readonly ApplicationInfo _info = new("RomTranslator", "\u00A9 2026 Patrick JAILLET", "0.1.0-test");

    private static MainViewModel Create(
        PortableLocations locations,
        FakeUrlLauncher launcher,
        Action? exit = null,
        AppSettings? settings = null,
        Action? openSettings = null,
        Action? openAbout = null,
        Func<NewSaturnProjectViewModel, string?>? openNewSaturnProjectWizard = null,
        Func<string?>? promptOpenProjectPath = null,
        Action<CharacterTableEditorViewModel>? openCharacterTableEditor = null,
        Func<string?>? promptSaveTranslatedRomPath = null,
        Func<string?>? promptSaveIpsPatchPath = null)
    {
        return new MainViewModel(
            _info,
            new SettingsStore(locations),
            settings ?? new AppSettings(),
            launcher,
            exit ?? (() => { }),
            "FR",
            locations.RootDirectory,
            locations.ProjectsDirectory,
            locations.TempDirectory,
            openSettings ?? (() => { }),
            openAbout ?? (() => { }),
            openNewSaturnProjectWizard ?? (_ => null),
            promptOpenProjectPath ?? (() => null),
            openCharacterTableEditor ?? (_ => { }),
            promptSaveTranslatedRomPath ?? (() => null),
            promptSaveIpsPatchPath ?? (() => null));
    }

    [Fact]
    public void Home_tab_is_first_selected_and_permanent()
    {
        using TemporaryDirectory temp = new();
        MainViewModel viewModel = Create(new PortableLocations(temp.FullPath), new FakeUrlLauncher());

        TabItemViewModel home = Assert.Single(viewModel.Tabs.Items);
        Assert.Equal(MainViewModel.HomeTabId, home.Id);
        Assert.True(home.IsPermanent);
        Assert.Same(home, viewModel.Tabs.SelectedItem);
        Assert.Same(viewModel.Home, home.Content);
    }

    [Fact]
    public void Title_is_the_product_name()
    {
        // Status.Message est résolu depuis les ressources RESX via CultureInfo.CurrentUICulture, jamais
        // définie par ce test (App.OnStartup s'en charge normalement) : sans cette précaution, le message
        // attendu dépend de la langue du système d'exécution (constaté en intégration continue, où le
        // runner Windows est en anglais alors que les postes de développement sont en français).
        CultureInfo previousUiCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr");
        try
        {
            using TemporaryDirectory temp = new();
            MainViewModel viewModel = Create(new PortableLocations(temp.FullPath), new FakeUrlLauncher());

            Assert.Equal("RomTranslator", viewModel.Title);
            Assert.Equal("Prêt", viewModel.Status.Message);
            Assert.Equal("FR", viewModel.Status.LanguageCode);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    [Fact]
    public void Features_not_yet_delivered_are_disabled()
    {
        using TemporaryDirectory temp = new();
        MainViewModel viewModel = Create(new PortableLocations(temp.FullPath), new FakeUrlLauncher());

        AppCommandViewModel[] pending = { viewModel.ExportProject, viewModel.Undo, viewModel.Redo };

        foreach (AppCommandViewModel command in pending)
        {
            Assert.False(command.Command.CanExecute(null), command.Header);
        }
    }

    [Fact]
    public void NewProject_and_OpenProject_are_enabled()
    {
        using TemporaryDirectory temp = new();
        MainViewModel viewModel = Create(new PortableLocations(temp.FullPath), new FakeUrlLauncher());

        Assert.True(viewModel.NewProject.Command.CanExecute(null));
        Assert.True(viewModel.OpenProject.Command.CanExecute(null));
    }

    [Fact]
    public void SaveProject_is_disabled_without_a_selected_project_tab()
    {
        using TemporaryDirectory temp = new();
        MainViewModel viewModel = Create(new PortableLocations(temp.FullPath), new FakeUrlLauncher());

        Assert.False(viewModel.SaveProject.Command.CanExecute(null));
    }

    [Fact]
    public void NewProject_invokes_the_wizard_and_opens_the_created_project_tab()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);
        bool wizardInvoked = false;

        MainViewModel viewModel = Create(
            locations,
            new FakeUrlLauncher(),
            openNewSaturnProjectWizard: wizard =>
            {
                wizardInvoked = true;
                Assert.NotNull(wizard);
                return null;
            });

        viewModel.NewProject.Command.Execute(null);

        Assert.True(wizardInvoked);
        Assert.Single(viewModel.Tabs.Items);
    }

    [Fact]
    public async Task End_to_end_create_translate_save_and_export_a_saturn_project()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);
        locations.EnsureDirectories();

        Directory.CreateDirectory(Path.Combine(locations.RootDirectory, "PredefinedTables"));
        File.Copy(
            Path.Combine(AppContext.BaseDirectory, "PredefinedTables", "ascii.tbl"),
            Path.Combine(locations.RootDirectory, "PredefinedTables", "ascii.tbl"),
            overwrite: true);

        byte[] payload = System.Text.Encoding.ASCII.GetBytes("Hello, adventurer!");
        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath, payloadBytes: payload, payloadOffset: 5L * 2048);

        string? createdProjectPath = null;
        string? romExportPath = null;
        string? patchExportPath = null;

        MainViewModel viewModel = Create(
            locations,
            new FakeUrlLauncher(),
            openNewSaturnProjectWizard: wizard =>
            {
                wizard.ValidateImageCommand.Execute(cuePath);
                wizard.ProjectName = "Mon jeu";
                wizard.ContinueToSettingsCommand.Execute(null);
                wizard.CreateProjectCommand.ExecuteAsync(null).GetAwaiter().GetResult();
                createdProjectPath = wizard.CreatedProjectPath;
                return createdProjectPath;
            },
            promptSaveTranslatedRomPath: () =>
            {
                romExportPath = Path.Combine(temp.FullPath, "export", "traduit.cue");
                return romExportPath;
            },
            promptSaveIpsPatchPath: () =>
            {
                patchExportPath = Path.Combine(temp.FullPath, "export", "patch.ips");
                return patchExportPath;
            });

        viewModel.NewProject.Command.Execute(null);

        Assert.NotNull(createdProjectPath);
        Assert.Equal(2, viewModel.Tabs.Items.Count);
        SaturnProjectViewModel projectTab = Assert.IsType<SaturnProjectViewModel>(viewModel.Tabs.SelectedItem?.Content);

        TranslationEntryViewModel entry = projectTab.Editor.Entries.Cast<TranslationEntryViewModel>()
            .Single(candidate => candidate.SourceText == "Hello, adventurer!");
        entry.TranslatedText = "Salut, voyageur!";

        Assert.True(viewModel.SaveProject.Command.CanExecute(null));
        viewModel.SaveProject.Command.Execute(null);

        TranslationProject reloaded = new TranslationProjectStore().Load(createdProjectPath!);
        Assert.Equal("Salut, voyageur!", reloaded.Entries.Single(e => e.SourceText == "Hello, adventurer!").TranslatedText);

        Directory.CreateDirectory(Path.Combine(temp.FullPath, "export"));
        await projectTab.ExportTranslatedRomCommand.ExecuteAsync(null);
        Assert.NotNull(romExportPath);
        Assert.True(File.Exists(romExportPath));

        await projectTab.ExportPatchCommand.ExecuteAsync(null);
        Assert.NotNull(patchExportPath);
        Assert.True(File.Exists(patchExportPath), viewModel.Status.Message);
    }

    [Fact]
    public void Settings_command_invokes_the_open_settings_action()
    {
        using TemporaryDirectory temp = new();
        int openCount = 0;
        MainViewModel viewModel = Create(new PortableLocations(temp.FullPath), new FakeUrlLauncher(), openSettings: () => openCount++);

        Assert.True(viewModel.Settings.Command.CanExecute(null));
        viewModel.Settings.Command.Execute(null);

        Assert.Equal(1, openCount);
    }

    [Fact]
    public void About_command_invokes_the_open_about_action()
    {
        using TemporaryDirectory temp = new();
        int openCount = 0;
        MainViewModel viewModel = Create(new PortableLocations(temp.FullPath), new FakeUrlLauncher(), openAbout: () => openCount++);

        Assert.True(viewModel.About.Command.CanExecute(null));
        viewModel.About.Command.Execute(null);

        Assert.Equal(1, openCount);
    }

    [Fact]
    public void Exit_command_invokes_the_exit_action()
    {
        using TemporaryDirectory temp = new();
        int exitCount = 0;
        MainViewModel viewModel = Create(new PortableLocations(temp.FullPath), new FakeUrlLauncher(), () => exitCount++);

        Assert.True(viewModel.Exit.Command.CanExecute(null));
        viewModel.Exit.Command.Execute(null);

        Assert.Equal(1, exitCount);
    }

    [Fact]
    public void Link_commands_open_their_address()
    {
        using TemporaryDirectory temp = new();
        FakeUrlLauncher launcher = new();
        MainViewModel viewModel = Create(new PortableLocations(temp.FullPath), launcher);

        viewModel.Website.Command.Execute(null);
        viewModel.Repository.Command.Execute(null);
        viewModel.Releases.Command.Execute(null);
        viewModel.Contact.Command.Execute(null);

        Assert.Equal(
            new[] { ProjectLinks.WebsiteUrl, ProjectLinks.RepositoryUrl, ProjectLinks.ReleasesUrl, ProjectLinks.ContactUrl },
            launcher.Opened.ToArray());
    }

    [Fact]
    public void Help_command_opens_the_website()
    {
        using TemporaryDirectory temp = new();
        FakeUrlLauncher launcher = new();
        MainViewModel viewModel = Create(new PortableLocations(temp.FullPath), launcher);

        viewModel.Help.Command.Execute(null);

        Assert.Equal(ProjectLinks.WebsiteUrl, Assert.Single(launcher.Opened));
    }

    [Fact]
    public void A_link_that_cannot_be_opened_is_reported_in_the_status_bar()
    {
        using TemporaryDirectory temp = new();
        MainViewModel viewModel = Create(new PortableLocations(temp.FullPath), new FakeUrlLauncher(succeeds: false));

        viewModel.Contact.Command.Execute(null);

        Assert.Contains(ProjectLinks.ContactUrl, viewModel.Status.Message);
    }

    [Fact]
    public void Home_lists_the_four_project_links()
    {
        using TemporaryDirectory temp = new();
        MainViewModel viewModel = Create(new PortableLocations(temp.FullPath), new FakeUrlLauncher());

        Assert.Equal(4, viewModel.Home.Links.Count);
        Assert.Equal("Version 0.1.0-test", viewModel.Home.VersionText);
    }

    [Fact]
    public void SaveState_persists_the_window_and_the_active_tab()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);
        MainViewModel viewModel = Create(locations, new FakeUrlLauncher());

        bool saved = viewModel.SaveState(new WindowSettings { Left = 12, Top = 34, Width = 1100, Height = 650, IsMaximized = true });

        Assert.True(saved);
        AppSettings loaded = new SettingsStore(locations).Load();
        Assert.Equal(12, loaded.Window.Left);
        Assert.Equal(1100, loaded.Window.Width);
        Assert.True(loaded.Window.IsMaximized);
        Assert.Equal(MainViewModel.HomeTabId, loaded.Window.LastActiveTabId);
    }

    [Fact]
    public void SaveState_returns_false_when_the_config_folder_is_not_writable()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);
        File.WriteAllText(locations.ConfigDirectory, "ce fichier bloque la création du dossier");
        MainViewModel viewModel = Create(locations, new FakeUrlLauncher());

        Assert.False(viewModel.SaveState(new WindowSettings()));
    }

    [Fact]
    public void The_tab_saved_in_settings_is_selected_when_it_is_registered()
    {
        using TemporaryDirectory temp = new();
        AppSettings settings = new();
        settings.Window.LastActiveTabId = "sega-saturn";
        MainViewModel viewModel = Create(new PortableLocations(temp.FullPath), new FakeUrlLauncher(), settings: settings);
        Assert.Equal(MainViewModel.HomeTabId, viewModel.Tabs.SelectedItem?.Id);

        viewModel.Tabs.AddTab(new TabItemViewModel("sega-saturn", "Sega Saturn", IconKeys.Console, new object()));

        Assert.Equal("sega-saturn", viewModel.Tabs.SelectedItem?.Id);
    }
}
