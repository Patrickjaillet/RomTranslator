// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;
using RomTranslator.App.ViewModels;
using RomTranslator.Core.Projects;
using RomTranslator.Modules.SegaSaturn;
using RomTranslator.Tests.SegaSaturn.Support;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.ViewModels;

public sealed class NewSaturnProjectViewModelTests
{
    private static NewSaturnProjectViewModel Create(string applicationDirectory, string projectsDirectory)
    {
        SaturnConsoleModule module = new(applicationDirectory, new TranslationProjectStore());
        return new NewSaturnProjectViewModel(module, new TranslationProjectStore(), applicationDirectory, projectsDirectory);
    }

    [Fact]
    public void ValidateImageCommand_accepts_a_valid_disc_image_and_advances_to_settings()
    {
        using TemporaryDirectory temp = new();
        Directory.CreateDirectory(Path.Combine(temp.FullPath, "app", "PredefinedTables"));
        File.Copy(
            Path.Combine(AppContext.BaseDirectory, "PredefinedTables", "ascii.tbl"),
            Path.Combine(temp.FullPath, "app", "PredefinedTables", "ascii.tbl"),
            overwrite: true);
        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath);

        NewSaturnProjectViewModel wizard = Create(Path.Combine(temp.FullPath, "app"), Path.Combine(temp.FullPath, "projects"));
        wizard.ValidateImageCommand.Execute(cuePath);

        Assert.True(wizard.IsRomValid);
        Assert.True(wizard.ContinueToSettingsCommand.CanExecute(null));
        Assert.Equal("test", wizard.ProjectName);
    }

    [Fact]
    public void ValidateImageCommand_rejects_an_image_without_the_saturn_hardware_id()
    {
        using TemporaryDirectory temp = new();
        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath, validHardwareId: false);

        NewSaturnProjectViewModel wizard = Create(temp.FullPath, temp.FullPath);
        wizard.ValidateImageCommand.Execute(cuePath);

        Assert.False(wizard.IsRomValid);
        Assert.NotEmpty(wizard.ValidationMessages);
        Assert.False(wizard.ContinueToSettingsCommand.CanExecute(null));
    }

    [Fact]
    public void CreateProjectCommand_extracts_text_and_saves_the_rtproj_file()
    {
        using TemporaryDirectory temp = new();
        string applicationDirectory = Path.Combine(temp.FullPath, "app");
        Directory.CreateDirectory(Path.Combine(applicationDirectory, "PredefinedTables"));
        File.Copy(
            Path.Combine(AppContext.BaseDirectory, "PredefinedTables", "ascii.tbl"),
            Path.Combine(applicationDirectory, "PredefinedTables", "ascii.tbl"),
            overwrite: true);

        byte[] payload = System.Text.Encoding.ASCII.GetBytes("Hello, adventurer!");
        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath, payloadBytes: payload, payloadOffset: 5L * 2048);

        string projectsDirectory = Path.Combine(temp.FullPath, "projects");
        Directory.CreateDirectory(projectsDirectory);

        NewSaturnProjectViewModel wizard = Create(applicationDirectory, projectsDirectory);
        wizard.ValidateImageCommand.Execute(cuePath);
        wizard.ProjectName = "Mon jeu";
        wizard.ContinueToSettingsCommand.Execute(null);

        Assert.True(wizard.CreateProjectCommand.CanExecute(null));
        wizard.CreateProjectCommand.Execute(null);

        Assert.Equal(NewSaturnProjectStep.Done, wizard.Step);
        Assert.NotNull(wizard.CreatedProjectPath);
        Assert.True(File.Exists(wizard.CreatedProjectPath));

        TranslationProject saved = new TranslationProjectStore().Load(wizard.CreatedProjectPath!);
        Assert.Equal("Mon jeu", saved.Name);
        Assert.Equal(SaturnConsoleModule.ModuleId, saved.ConsoleId);
        Assert.Contains(saved.Entries, entry => entry.SourceText == "Hello, adventurer!");
    }

    [Fact]
    public void CreateProjectCommand_cannot_execute_without_a_project_name()
    {
        using TemporaryDirectory temp = new();
        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath);

        NewSaturnProjectViewModel wizard = Create(temp.FullPath, temp.FullPath);
        wizard.ValidateImageCommand.Execute(cuePath);
        wizard.ProjectName = string.Empty;

        Assert.False(wizard.CreateProjectCommand.CanExecute(null));
    }

    [Fact]
    public void CreateProjectCommand_requires_an_external_path_when_that_table_choice_is_selected()
    {
        using TemporaryDirectory temp = new();
        string cuePath = SyntheticSaturnDiscBuilder.Build(temp.FullPath);

        NewSaturnProjectViewModel wizard = Create(temp.FullPath, temp.FullPath);
        wizard.ValidateImageCommand.Execute(cuePath);
        wizard.ProjectName = "Mon jeu";
        wizard.CharacterTableChoice = SaturnCharacterTableChoice.ExternalFile;

        Assert.False(wizard.CreateProjectCommand.CanExecute(null));

        wizard.ExternalCharacterTablePath = Path.Combine(temp.FullPath, "custom.tbl");
        Assert.True(wizard.CreateProjectCommand.CanExecute(null));
    }
}
