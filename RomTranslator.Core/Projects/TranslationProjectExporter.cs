// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.IO;

namespace RomTranslator.Core.Projects;

/// <summary>
/// Exporte et importe des projets de traduction pour le partage entre utilisateurs. L'export produit un
/// fichier <c>.rtproj</c> autonome, sans les sauvegardes incrémentales locales ; le chemin de ROM est
/// relativisé au dossier d'export quand la ROM se trouve sous ce dossier, pour rester portable d'une
/// machine à l'autre.
/// </summary>
public sealed class TranslationProjectExporter
{
    private readonly TranslationProjectStore _store;

    /// <summary>Initialise l'exporteur/importeur de projets.</summary>
    public TranslationProjectExporter(TranslationProjectStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        _store = store;
    }

    /// <summary>Exporte un projet vers un fichier <c>.rtproj</c> autonome, prêt à être partagé.</summary>
    /// <param name="project">Projet à exporter.</param>
    /// <param name="sourcePath">Chemin du fichier projet d'origine, utilisé pour relativiser le chemin de ROM.</param>
    /// <param name="destinationPath">Chemin du fichier d'export à créer.</param>
    public void Export(TranslationProject project, string sourcePath, string destinationPath)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);

        TranslationProject exported = Clone(project);
        exported.RomPath = RelativizeRomPath(project, sourcePath, destinationPath);

        _store.Save(exported, destinationPath);
    }

    /// <summary>Importe un projet depuis un fichier <c>.rtproj</c> exporté par un autre utilisateur.</summary>
    /// <param name="sourcePath">Chemin du fichier exporté à importer.</param>
    public TranslationProject Import(string sourcePath)
    {
        return _store.Load(sourcePath);
    }

    private static TranslationProject Clone(TranslationProject project)
    {
        return new TranslationProject
        {
            SchemaVersion = project.SchemaVersion,
            Name = project.Name,
            ConsoleId = project.ConsoleId,
            RomPath = project.RomPath,
            CharacterTableName = project.CharacterTableName,
            CreatedAt = project.CreatedAt,
            ModifiedAt = project.ModifiedAt,
            Entries = new List<TranslationEntry>(project.Entries),
        };
    }

    private static string RelativizeRomPath(TranslationProject project, string sourcePath, string destinationPath)
    {
        try
        {
            string? sourceDirectory = Path.GetDirectoryName(Path.GetFullPath(sourcePath));
            string? destinationDirectory = Path.GetDirectoryName(Path.GetFullPath(destinationPath));
            if (sourceDirectory is null || destinationDirectory is null)
            {
                return project.RomPath;
            }

            string absoluteRomPath = Path.IsPathRooted(project.RomPath)
                ? project.RomPath
                : Path.GetFullPath(Path.Combine(sourceDirectory, project.RomPath));

            if (!absoluteRomPath.StartsWith(sourceDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return project.RomPath;
            }

            return Path.GetRelativePath(destinationDirectory, absoluteRomPath);
        }
        catch (ArgumentException)
        {
            return project.RomPath;
        }
    }
}
