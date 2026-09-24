// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.Core.Projects;
using Xunit;

namespace RomTranslator.Tests.Projects;

public sealed class TranslationProjectFactoryTests
{
    [Fact]
    public void Create_initializes_an_empty_project()
    {
        TranslationProject project = TranslationProjectFactory.Create("Mon jeu", "sega-saturn", "roms/mon-jeu.cue");

        Assert.Equal("Mon jeu", project.Name);
        Assert.Equal("sega-saturn", project.ConsoleId);
        Assert.Equal("roms/mon-jeu.cue", project.RomPath);
        Assert.Empty(project.Entries);
        Assert.Equal(TranslationProject.CurrentSchemaVersion, project.SchemaVersion);
        Assert.Null(project.CharacterTableName);
        Assert.Equal(project.CreatedAt, project.ModifiedAt);
    }

    [Theory]
    [InlineData(null, "sega-saturn", "rom.cue")]
    [InlineData("", "sega-saturn", "rom.cue")]
    [InlineData("Nom", null, "rom.cue")]
    [InlineData("Nom", "sega-saturn", null)]
    public void Create_rejects_missing_arguments(string? name, string? consoleId, string? romPath)
    {
        Assert.ThrowsAny<ArgumentException>(() => TranslationProjectFactory.Create(name!, consoleId!, romPath!));
    }
}
