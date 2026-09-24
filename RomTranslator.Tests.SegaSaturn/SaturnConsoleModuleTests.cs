// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;
using RomTranslator.Core.Projects;
using RomTranslator.Modules.SegaSaturn;
using RomTranslator.Modules.SegaSaturn.CharacterTables;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn;

public sealed class SaturnConsoleModuleTests
{
    private static string CopyAsciiTableInto(string applicationDirectory)
    {
        string predefinedDirectory = Path.Combine(applicationDirectory, "PredefinedTables");
        Directory.CreateDirectory(predefinedDirectory);

        string sourcePath = Path.Combine(AppContext.BaseDirectory, "PredefinedTables", PredefinedCharacterTables.AsciiFileName);
        string destinationPath = Path.Combine(predefinedDirectory, PredefinedCharacterTables.AsciiFileName);
        File.Copy(sourcePath, destinationPath, overwrite: true);

        return applicationDirectory;
    }

    [Fact]
    public void Id_is_the_stable_module_identifier()
    {
        using TemporaryDirectory temp = new();
        SaturnConsoleModule module = new(CopyAsciiTableInto(temp.FullPath), new TranslationProjectStore());

        Assert.Equal("sega-saturn", module.Id);
        Assert.Equal(SaturnConsoleModule.ModuleId, module.Id);
    }

    [Fact]
    public void LoadProjectContext_resolves_the_ascii_table_by_default()
    {
        using TemporaryDirectory temp = new();
        string applicationDirectory = CopyAsciiTableInto(temp.FullPath);
        TranslationProjectStore store = new();
        SaturnConsoleModule module = new(applicationDirectory, store);

        TranslationProject project = TranslationProjectFactory.Create("Mon jeu", SaturnConsoleModule.ModuleId, "rom.cue");
        string projectPath = Path.Combine(temp.FullPath, "projet.rtproj");
        store.Save(project, projectPath);

        ConsoleProjectContext context = module.LoadProjectContext(projectPath);

        Assert.Equal("Mon jeu", context.Project.Name);
        Assert.NotNull(context.LengthPolicy);
        Assert.Equal("ASCII", context.CharacterTable.Name);
    }

    [Fact]
    public void LoadProjectContext_resolves_the_shift_jis_table_when_named_by_the_project()
    {
        using TemporaryDirectory temp = new();
        string applicationDirectory = temp.FullPath;
        Directory.CreateDirectory(Path.Combine(applicationDirectory, "PredefinedTables"));
        File.Copy(
            Path.Combine(AppContext.BaseDirectory, "PredefinedTables", PredefinedCharacterTables.ShiftJisKanaFileName),
            Path.Combine(applicationDirectory, "PredefinedTables", PredefinedCharacterTables.ShiftJisKanaFileName),
            overwrite: true);

        TranslationProjectStore store = new();
        SaturnConsoleModule module = new(applicationDirectory, store);

        TranslationProject project = TranslationProjectFactory.Create("Mon jeu", SaturnConsoleModule.ModuleId, "rom.cue");
        project.CharacterTableName = PredefinedCharacterTables.ShiftJisKanaFileName;
        string projectPath = Path.Combine(temp.FullPath, "projet.rtproj");
        store.Save(project, projectPath);

        ConsoleProjectContext context = module.LoadProjectContext(projectPath);

        Assert.Equal("Shift-JIS (kana)", context.CharacterTable.Name);
    }

    [Fact]
    public void LoadProjectContext_loads_an_external_character_table_file()
    {
        using TemporaryDirectory temp = new();
        string applicationDirectory = CopyAsciiTableInto(temp.FullPath);
        string externalTablePath = Path.Combine(temp.FullPath, "custom.tbl");
        File.WriteAllLines(externalTablePath, new[] { "41=A", "42=B" });

        TranslationProjectStore store = new();
        SaturnConsoleModule module = new(applicationDirectory, store);

        TranslationProject project = TranslationProjectFactory.Create("Mon jeu", SaturnConsoleModule.ModuleId, "rom.cue");
        project.CharacterTableName = externalTablePath;
        string projectPath = Path.Combine(temp.FullPath, "projet.rtproj");
        store.Save(project, projectPath);

        ConsoleProjectContext context = module.LoadProjectContext(projectPath);

        Assert.Equal("AB", context.CharacterTable.Decode(new byte[] { 0x41, 0x42 }));
    }

    [Fact]
    public void Constructor_rejects_null_arguments()
    {
        Assert.Throws<ArgumentException>(() => new SaturnConsoleModule(string.Empty, new TranslationProjectStore()));
        Assert.Throws<ArgumentNullException>(() => new SaturnConsoleModule("dir", null!));
    }
}
