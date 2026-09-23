// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.IO;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Projects;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.Projects;

public sealed class TranslationProjectExporterTests
{
    [Fact]
    public void Export_then_Import_round_trips_the_project_content()
    {
        using TemporaryDirectory source = new();
        using TemporaryDirectory destination = new();
        TranslationProjectStore store = new();
        TranslationProjectExporter exporter = new(store);

        string sourcePath = Path.Combine(source.FullPath, "mon-jeu" + TranslationProjectStore.FileExtension);
        TranslationProject project = TranslationProjectFactory.Create("Mon jeu", "sega-saturn", "mon-jeu.cue");
        project.Entries.Add(new TranslationEntry("e1", "Hello", 0, translatedText: "Bonjour", status: TranslationStatus.Validated));
        store.Save(project, sourcePath);

        string exportPath = Path.Combine(destination.FullPath, "mon-jeu-export" + TranslationProjectStore.FileExtension);
        exporter.Export(project, sourcePath, exportPath);
        TranslationProject imported = exporter.Import(exportPath);

        Assert.Equal("Mon jeu", imported.Name);
        Assert.Equal("sega-saturn", imported.ConsoleId);
        TranslationEntry entry = Assert.Single(imported.Entries);
        Assert.Equal("Bonjour", entry.TranslatedText);
    }

    [Fact]
    public void Export_relativizes_a_rom_path_located_under_the_source_project_folder()
    {
        using TemporaryDirectory source = new();
        using TemporaryDirectory destination = new();
        TranslationProjectStore store = new();
        TranslationProjectExporter exporter = new(store);

        Directory.CreateDirectory(Path.Combine(source.FullPath, "roms"));
        string sourcePath = Path.Combine(source.FullPath, "mon-jeu" + TranslationProjectStore.FileExtension);
        TranslationProject project = TranslationProjectFactory.Create("Mon jeu", "sega-saturn", Path.Combine("roms", "mon-jeu.cue"));
        store.Save(project, sourcePath);

        string exportPath = Path.Combine(destination.FullPath, "mon-jeu-export" + TranslationProjectStore.FileExtension);
        exporter.Export(project, sourcePath, exportPath);
        TranslationProject imported = exporter.Import(exportPath);

        Assert.False(Path.IsPathRooted(imported.RomPath));
    }

    [Fact]
    public void Export_does_not_modify_the_original_project_in_memory()
    {
        using TemporaryDirectory source = new();
        using TemporaryDirectory destination = new();
        TranslationProjectStore store = new();
        TranslationProjectExporter exporter = new(store);

        string sourcePath = Path.Combine(source.FullPath, "mon-jeu" + TranslationProjectStore.FileExtension);
        TranslationProject project = TranslationProjectFactory.Create("Mon jeu", "sega-saturn", "mon-jeu.cue");
        store.Save(project, sourcePath);
        string originalRomPath = project.RomPath;

        exporter.Export(project, sourcePath, Path.Combine(destination.FullPath, "export" + TranslationProjectStore.FileExtension));

        Assert.Equal(originalRomPath, project.RomPath);
    }
}
