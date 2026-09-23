// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;
using RomTranslator.Core.Portability;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.Portability;

public sealed class PortableLocationsTests
{
    [Fact]
    public void Folders_are_located_under_the_root()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);

        Assert.Equal(temp.FullPath, locations.RootDirectory);
        Assert.Equal(Path.Combine(temp.FullPath, "config"), locations.ConfigDirectory);
        Assert.Equal(Path.Combine(temp.FullPath, "projects"), locations.ProjectsDirectory);
        Assert.Equal(Path.Combine(temp.FullPath, "logs"), locations.LogsDirectory);
        Assert.Equal(Path.Combine(temp.FullPath, "config", "settings.json"), locations.SettingsFilePath);
    }

    [Fact]
    public void EnsureDirectories_creates_the_three_folders()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);

        locations.EnsureDirectories();

        Assert.True(Directory.Exists(locations.ConfigDirectory));
        Assert.True(Directory.Exists(locations.ProjectsDirectory));
        Assert.True(Directory.Exists(locations.LogsDirectory));
    }

    [Fact]
    public void Trailing_separator_of_the_root_is_ignored()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath + Path.DirectorySeparatorChar);

        Assert.Equal(temp.FullPath, locations.RootDirectory);
    }

    [Fact]
    public void Resolve_returns_a_path_inside_the_root()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);

        string resolved = locations.Resolve("projects/game/project.rtproj");

        Assert.Equal(Path.Combine(temp.FullPath, "projects", "game", "project.rtproj"), resolved);
    }

    [Theory]
    [InlineData("..\\outside.txt")]
    [InlineData("config/../../outside.txt")]
    [InlineData("projects/../../../outside.txt")]
    public void Resolve_rejects_paths_leaving_the_root(string relativePath)
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(Path.Combine(temp.FullPath, "app"));

        Assert.Throws<ArgumentException>(() => locations.Resolve(relativePath));
    }

    [Fact]
    public void Resolve_rejects_absolute_paths()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);

        Assert.Throws<ArgumentException>(() => locations.Resolve(Path.Combine(temp.FullPath, "config")));
    }

    [Fact]
    public void IsInsideRoot_rejects_a_sibling_folder_sharing_the_same_prefix()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(Path.Combine(temp.FullPath, "App"));

        Assert.True(locations.IsInsideRoot(Path.Combine(temp.FullPath, "App", "config")));
        Assert.True(locations.IsInsideRoot(Path.Combine(temp.FullPath, "App")));
        Assert.False(locations.IsInsideRoot(Path.Combine(temp.FullPath, "App2", "config")));
    }

    [Fact]
    public void Constructor_rejects_an_empty_root()
    {
        Assert.Throws<ArgumentException>(() => new PortableLocations("  "));
    }

    [Fact]
    public void FromApplicationBase_uses_the_application_folder()
    {
        string expected = Path.TrimEndingDirectorySeparator(Path.GetFullPath(AppContext.BaseDirectory));

        Assert.Equal(expected, PortableLocations.FromApplicationBase().RootDirectory);
    }
}
