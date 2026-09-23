// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;
using RomTranslator.App.ViewModels;
using RomTranslator.Core.Configuration;
using RomTranslator.Core.Information;
using RomTranslator.Core.Portability;
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
        AppSettings? settings = null)
    {
        return new MainViewModel(_info, new SettingsStore(locations), settings ?? new AppSettings(), launcher, exit ?? (() => { }), "FR");
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
        using TemporaryDirectory temp = new();
        MainViewModel viewModel = Create(new PortableLocations(temp.FullPath), new FakeUrlLauncher());

        Assert.Equal("RomTranslator", viewModel.Title);
        Assert.Equal("Prêt", viewModel.Status.Message);
        Assert.Equal("FR", viewModel.Status.LanguageCode);
    }

    [Fact]
    public void Features_not_yet_delivered_are_disabled()
    {
        using TemporaryDirectory temp = new();
        MainViewModel viewModel = Create(new PortableLocations(temp.FullPath), new FakeUrlLauncher());

        AppCommandViewModel[] pending =
        {
            viewModel.NewProject, viewModel.OpenProject, viewModel.SaveProject, viewModel.ExportProject,
            viewModel.Undo, viewModel.Redo, viewModel.Settings, viewModel.About,
        };

        foreach (AppCommandViewModel command in pending)
        {
            Assert.False(command.Command.CanExecute(null), command.Header);
        }
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
