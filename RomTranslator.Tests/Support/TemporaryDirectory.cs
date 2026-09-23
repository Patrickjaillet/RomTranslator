// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;

namespace RomTranslator.Tests.Support;

/// <summary>Dossier temporaire supprimé à la fin du test.</summary>
internal sealed class TemporaryDirectory : IDisposable
{
    public TemporaryDirectory()
    {
        FullPath = Directory.CreateTempSubdirectory("RomTranslator.Tests-").FullName;
    }

    public string FullPath { get; }

    public void Dispose()
    {
        try
        {
            Directory.Delete(FullPath, recursive: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Le nettoyage d'un dossier temporaire ne doit jamais faire échouer un test.
        }
    }
}
