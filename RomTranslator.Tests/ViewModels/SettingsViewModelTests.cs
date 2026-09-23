// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Linq;
using RomTranslator.App.ViewModels;
using RomTranslator.Core.Configuration;
using RomTranslator.Core.Portability;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.ViewModels;

public sealed class SettingsViewModelTests
{
    private static SettingsViewModel Create(
        PortableLocations locations,
        AppSettings? settings = null,
        FakeFolderLauncher? folderLauncher = null,
        System.Action<string>? reportMessage = null)
    {
        return new SettingsViewModel(
            new SettingsStore(locations),
            settings ?? new AppSettings(),
            locations,
            folderLauncher ?? new FakeFolderLauncher(),
            reportMessage ?? (_ => { }));
    }

    [Fact]
    public void LanguageOptions_offers_system_French_and_English()
    {
        using TemporaryDirectory temp = new();
        SettingsViewModel viewModel = Create(new PortableLocations(temp.FullPath));

        Assert.Equal(3, viewModel.LanguageOptions.Count);
        Assert.Contains(viewModel.LanguageOptions, option => option.LanguageCode is null);
        Assert.Contains(viewModel.LanguageOptions, option => option.LanguageCode == "fr");
        Assert.Contains(viewModel.LanguageOptions, option => option.LanguageCode == "en");
    }

    [Fact]
    public void SelectedLanguage_defaults_to_the_language_saved_in_settings()
    {
        using TemporaryDirectory temp = new();
        AppSettings settings = new() { LanguageCode = "en" };
        SettingsViewModel viewModel = Create(new PortableLocations(temp.FullPath), settings);

        Assert.Equal("en", viewModel.SelectedLanguage.LanguageCode);
    }

    [Fact]
    public void SelectedLanguage_defaults_to_system_when_no_language_is_saved()
    {
        using TemporaryDirectory temp = new();
        SettingsViewModel viewModel = Create(new PortableLocations(temp.FullPath));

        Assert.Null(viewModel.SelectedLanguage.LanguageCode);
    }

    [Fact]
    public void SaveCommand_persists_the_selected_language()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);
        SettingsViewModel viewModel = Create(locations);

        viewModel.SelectedLanguage = viewModel.LanguageOptions.Single(option => option.LanguageCode == "en");
        viewModel.SaveCommand.Execute(null);

        Assert.True(viewModel.IsSaved);
        AppSettings loaded = new SettingsStore(locations).Load();
        Assert.Equal("en", loaded.LanguageCode);
    }

    [Fact]
    public void OpenConfigFolderCommand_opens_the_configuration_directory()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);
        FakeFolderLauncher launcher = new();
        SettingsViewModel viewModel = Create(locations, folderLauncher: launcher);

        viewModel.OpenConfigFolderCommand.Execute(null);

        Assert.Equal(locations.ConfigDirectory, Assert.Single(launcher.Opened));
    }

    [Fact]
    public void A_folder_that_cannot_be_opened_is_reported()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);
        string? reported = null;
        SettingsViewModel viewModel = Create(locations, folderLauncher: new FakeFolderLauncher(succeeds: false), reportMessage: message => reported = message);

        viewModel.OpenProjectsFolderCommand.Execute(null);

        Assert.Contains(locations.ProjectsDirectory, reported);
    }
}
