// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;
using RomTranslator.Core.Diagnostics;
using RomTranslator.Core.Portability;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.Diagnostics;

public sealed class CrashReporterTests
{
    [Fact]
    public void TryWrite_creates_a_report_in_the_logs_folder()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);
        InvalidOperationException exception = new("échec de démonstration");

        string? path = CrashReporter.TryWrite(locations, exception, new DateTimeOffset(2026, 9, 23, 14, 30, 5, TimeSpan.Zero));

        Assert.NotNull(path);
        Assert.Equal(locations.LogsDirectory, Path.GetDirectoryName(path));
        Assert.Equal("crash-20260923-143005-000.log", Path.GetFileName(path));

        string content = File.ReadAllText(path);
        Assert.Contains("InvalidOperationException", content);
        Assert.Contains("échec de démonstration", content);
    }

    [Fact]
    public void TryWrite_returns_null_when_the_logs_folder_cannot_be_created()
    {
        using TemporaryDirectory temp = new();
        PortableLocations locations = new(temp.FullPath);
        File.WriteAllText(locations.LogsDirectory, "ce fichier bloque la création du dossier");

        string? path = CrashReporter.TryWrite(locations, new InvalidOperationException("test"));

        Assert.Null(path);
    }
}
