// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RomTranslator.Tests.Support;

/// <summary>Localise la racine du dépôt (dossier contenant RomTranslator.sln) pour les contrôles de conformité.</summary>
internal static class RepositoryLocator
{
    private static readonly string[] _excludedFolders = { ".git", ".vs", "bin", "obj", "publish", "TestResults" };
    private static readonly Lazy<string> _root = new(FindRoot);

    public static string Root => _root.Value;

    /// <summary>Énumère les fichiers d'un dossier du dépôt, en ignorant les dossiers générés (bin, obj, .git...).</summary>
    /// <param name="relativeFolder">Dossier relatif à la racine (« . » pour toute la racine).</param>
    /// <param name="searchPattern">Motif de nom de fichier.</param>
    public static IEnumerable<string> EnumerateFiles(string relativeFolder, string searchPattern)
    {
        string folder = Path.GetFullPath(Path.Combine(Root, relativeFolder));

        return Directory.EnumerateFiles(folder, searchPattern, SearchOption.AllDirectories)
            .Where(path => !IsInExcludedFolder(path));
    }

    private static bool IsInExcludedFolder(string path)
    {
        string relative = Path.GetRelativePath(Root, path);
        string[] segments = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return segments.Take(segments.Length - 1).Any(segment => _excludedFolders.Contains(segment, StringComparer.OrdinalIgnoreCase));
    }

    private static string FindRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "RomTranslator.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException(
            "Racine du dépôt introuvable : RomTranslator.sln n'existe dans aucun dossier parent de " + AppContext.BaseDirectory);
    }
}
