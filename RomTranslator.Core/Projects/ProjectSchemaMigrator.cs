// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;

namespace RomTranslator.Core.Projects;

/// <summary>
/// Migre un fichier de projet chargé vers la version courante du format (<see cref="TranslationProject.CurrentSchemaVersion" />).
/// Chaque montée de version doit ajouter ici l'étape de migration correspondante, jamais modifier une
/// étape déjà publiée.
/// </summary>
public static class ProjectSchemaMigrator
{
    /// <summary>Migre un projet chargé vers la version courante du format.</summary>
    /// <exception cref="NotSupportedException">Le projet a été enregistré par une version plus récente de l'application.</exception>
    public static TranslationProject MigrateToCurrent(TranslationProject project)
    {
        ArgumentNullException.ThrowIfNull(project);

        if (project.SchemaVersion > TranslationProject.CurrentSchemaVersion)
        {
            throw new NotSupportedException(
                $"Ce projet a été enregistré par une version plus récente de RomTranslator (format {project.SchemaVersion}, "
                + $"cette version ne prend en charge que jusqu'au format {TranslationProject.CurrentSchemaVersion}).");
        }

        // Aucune migration nécessaire pour l'instant : une seule version de format existe (1).
        // Une future version 2 ajouterait ici, par exemple : if (project.SchemaVersion < 2) { ... ; project.SchemaVersion = 2; }
        project.SchemaVersion = TranslationProject.CurrentSchemaVersion;

        return project;
    }
}
