// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.IO;
using RomTranslator.Core.Diagnostics;
using RomTranslator.Core.Portability;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.Diagnostics;

public sealed class ActivityLoggerTests
{
    [Fact]
    public void Log_writes_a_line_to_the_current_day_log_file()
    {
        using TemporaryDirectory temp = new();
        ActivityLogger logger = new(new PortableLocations(temp.FullPath));

        logger.Log(LogLevel.Information, "Projet ouvert");

        string expectedPath = Path.Combine(
            temp.FullPath, "logs", "activity-" + DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".log");
        Assert.True(File.Exists(expectedPath));
        Assert.Contains("Projet ouvert", File.ReadAllText(expectedPath));
        Assert.Contains("[INFORMATION]", File.ReadAllText(expectedPath));
    }

    [Fact]
    public void Log_ignores_messages_below_the_minimum_level()
    {
        using TemporaryDirectory temp = new();
        ActivityLogger logger = new(new PortableLocations(temp.FullPath), minimumLevel: LogLevel.Warning);

        logger.Log(LogLevel.Information, "Ne doit pas apparaître");

        Assert.False(File.Exists(logger.CurrentLogFilePath));
    }

    [Fact]
    public void Log_appends_successive_messages_to_the_same_file()
    {
        using TemporaryDirectory temp = new();
        ActivityLogger logger = new(new PortableLocations(temp.FullPath));

        logger.Log(LogLevel.Information, "Premier message");
        logger.Log(LogLevel.Information, "Second message");

        string[] lines = File.ReadAllLines(logger.CurrentLogFilePath);
        Assert.Equal(2, lines.Length);
    }

    [Fact]
    public void MinimumLevel_can_be_changed_after_construction()
    {
        using TemporaryDirectory temp = new();
        ActivityLogger logger = new(new PortableLocations(temp.FullPath), minimumLevel: LogLevel.Error);

        logger.Log(LogLevel.Warning, "Ignoré");
        Assert.False(File.Exists(logger.CurrentLogFilePath));

        logger.MinimumLevel = LogLevel.Warning;
        logger.Log(LogLevel.Warning, "Consigné");
        Assert.True(File.Exists(logger.CurrentLogFilePath));
    }

    [Fact]
    public void Log_does_not_throw_when_the_logs_folder_cannot_be_created()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);
        File.WriteAllText(locations.LogsDirectory, "ce fichier bloque la création du dossier");
        ActivityLogger logger = new(locations);

        Exception? exception = Record.Exception(() => logger.Log(LogLevel.Information, "test"));

        Assert.Null(exception);
    }

    [Fact]
    public void PruneOldLogFiles_keeps_only_the_most_recent_files()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);
        Directory.CreateDirectory(locations.LogsDirectory);
        for (int day = 1; day <= 20; day++)
        {
            File.WriteAllText(Path.Combine(locations.LogsDirectory, $"activity-2026-01-{day:D2}.log"), "contenu");
        }

        ActivityLogger logger = new(locations, retainedDays: 5);
        logger.PruneOldLogFiles();

        string[] remaining = Directory.GetFiles(locations.LogsDirectory, "activity-*.log");
        Assert.Equal(5, remaining.Length);
        Assert.Contains(remaining, file => file.Contains("2026-01-20", StringComparison.Ordinal));
        Assert.DoesNotContain(remaining, file => file.Contains("2026-01-01", StringComparison.Ordinal));
    }

    [Fact]
    public void PruneOldLogFiles_does_not_throw_when_the_logs_folder_does_not_exist()
    {
        using TemporaryDirectory temp = new();
        ActivityLogger logger = new(new PortableLocations(temp.FullPath));

        Exception? exception = Record.Exception(logger.PruneOldLogFiles);

        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_rejects_a_retained_days_count_below_one()
    {
        using TemporaryDirectory temp = new();

        Assert.Throws<ArgumentOutOfRangeException>(() => new ActivityLogger(new PortableLocations(temp.FullPath), retainedDays: 0));
    }
}
