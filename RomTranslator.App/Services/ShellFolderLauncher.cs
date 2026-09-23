// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace RomTranslator.App.Services;

/// <summary>Ouvre un dossier local dans l'explorateur Windows via le shell, en créant le dossier au besoin.</summary>
public sealed class ShellFolderLauncher : IFolderLauncher
{
    /// <inheritdoc />
    public bool TryOpen(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            Directory.CreateDirectory(path);
            using Process? process = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            return true;
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
