// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.IO;
using RomTranslator.Core.Configuration;
using RomTranslator.Core.Portability;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.Configuration;

public sealed class SettingsStoreTests
{
    [Fact]
    public void Load_returns_defaults_when_the_file_is_missing()
    {
        using TemporaryDirectory temp = new();
        SettingsStore store = new(new PortableLocations(temp.FullPath));

        AppSettings settings = store.Load();

        Assert.Equal(AppSettings.CurrentSchemaVersion, settings.SchemaVersion);
        Assert.Null(settings.Window.Left);
        Assert.Null(settings.Window.LastActiveTabId);
        Assert.False(settings.Window.IsMaximized);
    }

    [Fact]
    public void Save_then_Load_round_trips_the_window_state()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);
        AppSettings settings = new();
        settings.Window.Left = 10.5;
        settings.Window.Top = 20;
        settings.Window.Width = 1000;
        settings.Window.Height = 700;
        settings.Window.IsMaximized = true;
        settings.Window.LastActiveTabId = "sega-saturn";

        new SettingsStore(locations).Save(settings);
        AppSettings loaded = new SettingsStore(locations).Load();

        Assert.Equal(10.5, loaded.Window.Left);
        Assert.Equal(20, loaded.Window.Top);
        Assert.Equal(1000, loaded.Window.Width);
        Assert.Equal(700, loaded.Window.Height);
        Assert.True(loaded.Window.IsMaximized);
        Assert.Equal("sega-saturn", loaded.Window.LastActiveTabId);
    }

    [Fact]
    public void Save_creates_the_config_folder_and_leaves_no_temporary_file()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);

        new SettingsStore(locations).Save(new AppSettings());

        Assert.True(File.Exists(locations.SettingsFilePath));
        Assert.Empty(Directory.GetFiles(locations.ConfigDirectory, "*.tmp"));
    }

    [Fact]
    public void Save_overwrites_an_existing_file()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);
        SettingsStore store = new(locations);
        AppSettings first = new();
        first.Window.LastActiveTabId = "first";
        AppSettings second = new();
        second.Window.LastActiveTabId = "second";

        store.Save(first);
        store.Save(second);

        Assert.Equal("second", store.Load().Window.LastActiveTabId);
    }

    [Fact]
    public void Load_keeps_a_corrupt_file_aside_and_returns_defaults()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);
        locations.EnsureDirectories();
        File.WriteAllText(locations.SettingsFilePath, "{ ceci n'est pas du JSON");

        AppSettings settings = new SettingsStore(locations).Load();

        Assert.Null(settings.Window.Left);
        Assert.False(File.Exists(locations.SettingsFilePath));
        Assert.Single(Directory.GetFiles(locations.ConfigDirectory, "settings.json.corrupt-*"));
    }

    [Fact]
    public void Load_tolerates_a_null_window_section()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);
        locations.EnsureDirectories();
        File.WriteAllText(locations.SettingsFilePath, "{\"schemaVersion\":1,\"window\":null}");

        AppSettings settings = new SettingsStore(locations).Load();

        Assert.NotNull(settings.Window);
    }
}
