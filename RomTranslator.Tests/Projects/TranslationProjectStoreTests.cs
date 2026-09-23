// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;
using System.Linq;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Projects;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.Projects;

public sealed class TranslationProjectStoreTests
{
    [Fact]
    public void Save_then_Load_round_trips_the_project()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "mon-jeu" + TranslationProjectStore.FileExtension);
        TranslationProjectStore store = new();

        TranslationProject project = TranslationProjectFactory.Create("Mon jeu", "sega-saturn", "mon-jeu.cue");
        project.CharacterTableName = "shift-jis.tbl";
        project.Entries.Add(new TranslationEntry("e1", "Hello", 0x100, translatedText: "Bonjour", status: TranslationStatus.Translated));

        store.Save(project, path);
        TranslationProject loaded = store.Load(path);

        Assert.Equal("Mon jeu", loaded.Name);
        Assert.Equal("sega-saturn", loaded.ConsoleId);
        Assert.Equal("mon-jeu.cue", loaded.RomPath);
        Assert.Equal("shift-jis.tbl", loaded.CharacterTableName);
        TranslationEntry entry = Assert.Single(loaded.Entries);
        Assert.Equal("e1", entry.Id);
        Assert.Equal("Bonjour", entry.TranslatedText);
        Assert.Equal(TranslationStatus.Translated, entry.Status);
    }

    [Fact]
    public void Save_creates_the_destination_folder()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "projects", "mon-jeu" + TranslationProjectStore.FileExtension);
        TranslationProjectStore store = new();

        store.Save(TranslationProjectFactory.Create("Mon jeu", "sega-saturn", "mon-jeu.cue"), path);

        Assert.True(File.Exists(path));
    }

    [Fact]
    public void Save_leaves_no_temporary_file()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "mon-jeu" + TranslationProjectStore.FileExtension);
        TranslationProjectStore store = new();

        store.Save(TranslationProjectFactory.Create("Mon jeu", "sega-saturn", "mon-jeu.cue"), path);

        Assert.Empty(Directory.GetFiles(temp.FullPath, "*.tmp"));
    }

    [Fact]
    public void Load_throws_when_the_file_is_missing()
    {
        using TemporaryDirectory temp = new();
        TranslationProjectStore store = new();

        Assert.Throws<FileNotFoundException>(() => store.Load(Path.Combine(temp.FullPath, "absent" + TranslationProjectStore.FileExtension)));
    }

    [Fact]
    public void Save_updates_ModifiedAt()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "mon-jeu" + TranslationProjectStore.FileExtension);
        TranslationProjectStore store = new();

        TranslationProject project = TranslationProjectFactory.Create("Mon jeu", "sega-saturn", "mon-jeu.cue");
        DateTimeOffset createdAt = project.CreatedAt;
        project.ModifiedAt = createdAt.AddDays(-1);

        store.Save(project, path);

        Assert.True(project.ModifiedAt >= createdAt);
    }

    [Fact]
    public void Save_creates_a_timestamped_backup_of_the_previous_version()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "mon-jeu" + TranslationProjectStore.FileExtension);
        TranslationProjectStore store = new();

        store.Save(TranslationProjectFactory.Create("Version 1", "sega-saturn", "mon-jeu.cue"), path);
        store.Save(TranslationProjectFactory.Create("Version 2", "sega-saturn", "mon-jeu.cue"), path);

        string backupDirectory = Path.Combine(temp.FullPath, TranslationProjectStore.BackupDirectoryName);
        string[] backups = Directory.GetFiles(backupDirectory, "*" + TranslationProjectStore.FileExtension);
        TranslationProject backedUp = Assert.Single(backups.Select(store.Load));
        Assert.Equal("Version 1", backedUp.Name);
    }

    [Fact]
    public void Save_does_not_create_a_backup_for_a_brand_new_project()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "mon-jeu" + TranslationProjectStore.FileExtension);
        TranslationProjectStore store = new();

        store.Save(TranslationProjectFactory.Create("Mon jeu", "sega-saturn", "mon-jeu.cue"), path);

        Assert.False(Directory.Exists(Path.Combine(temp.FullPath, TranslationProjectStore.BackupDirectoryName)));
    }

    [Fact]
    public void Save_prunes_backups_beyond_the_retention_limit()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "mon-jeu" + TranslationProjectStore.FileExtension);
        TranslationProjectStore store = new();

        for (int i = 0; i < TranslationProjectStore.MaxBackupCount + 5; i++)
        {
            store.Save(TranslationProjectFactory.Create("Version " + i, "sega-saturn", "mon-jeu.cue"), path);
        }

        string backupDirectory = Path.Combine(temp.FullPath, TranslationProjectStore.BackupDirectoryName);
        Assert.Equal(TranslationProjectStore.MaxBackupCount, Directory.GetFiles(backupDirectory).Length);
    }

    [Fact]
    public void Load_rejects_a_project_saved_by_a_newer_format()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "mon-jeu" + TranslationProjectStore.FileExtension);
        File.WriteAllText(
            path,
            "{\"schemaVersion\":" + (TranslationProject.CurrentSchemaVersion + 1) + ",\"name\":\"Futur\",\"consoleId\":\"x\",\"romPath\":\"r.cue\"}");
        TranslationProjectStore store = new();

        Assert.Throws<NotSupportedException>(() => store.Load(path));
    }
}
