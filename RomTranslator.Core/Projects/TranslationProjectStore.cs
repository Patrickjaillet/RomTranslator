// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RomTranslator.Core.Projects;

/// <summary>
/// Lecture et écriture des fichiers de projet de traduction (<c>.rtproj</c>, JSON versionné). Chaque
/// enregistrement est atomique (fichier temporaire puis remplacement) et conserve une sauvegarde
/// incrémentale horodatée dans un sous-dossier <c>.backups</c> à côté du fichier projet.
/// </summary>
public sealed class TranslationProjectStore
{
    /// <summary>Extension des fichiers de projet.</summary>
    public const string FileExtension = ".rtproj";

    /// <summary>Nom du sous-dossier de sauvegardes incrémentales, créé à côté de chaque fichier projet.</summary>
    public const string BackupDirectoryName = ".backups";

    /// <summary>Nombre de sauvegardes incrémentales conservées par projet.</summary>
    public const int MaxBackupCount = 10;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>Charge un projet depuis son fichier <c>.rtproj</c>.</summary>
    /// <param name="path">Chemin du fichier projet.</param>
    /// <exception cref="FileNotFoundException">Le fichier n'existe pas.</exception>
    /// <exception cref="JsonException">Le contenu du fichier n'est pas un projet valide.</exception>
    public TranslationProject Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Le fichier de projet est introuvable.", path);
        }

        using FileStream stream = File.OpenRead(path);
        TranslationProject project = JsonSerializer.Deserialize<TranslationProject>(stream, _jsonOptions)
            ?? throw new JsonException("Le fichier de projet est vide ou invalide.");

        return ProjectSchemaMigrator.MigrateToCurrent(project);
    }

    /// <summary>
    /// Enregistre un projet de façon atomique (fichier temporaire du même dossier puis remplacement) et
    /// dépose une sauvegarde incrémentale horodatée avant d'écraser un fichier existant.
    /// </summary>
    /// <param name="project">Projet à enregistrer.</param>
    /// <param name="path">Chemin du fichier projet.</param>
    /// <exception cref="IOException">Le dossier de destination n'est pas accessible en écriture.</exception>
    public void Save(TranslationProject project, string path)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        project.ModifiedAt = DateTimeOffset.UtcNow;

        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(path))
        {
            CreateBackup(path);
        }

        string temporary = path + ".tmp";
        using (FileStream stream = File.Create(temporary))
        {
            JsonSerializer.Serialize(stream, project, _jsonOptions);
        }

        File.Move(temporary, path, overwrite: true);
    }

    private static void CreateBackup(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(directory))
        {
            return;
        }

        string backupDirectory = Path.Combine(directory, BackupDirectoryName);
        Directory.CreateDirectory(backupDirectory);

        string stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
        string backupFileName = Path.GetFileNameWithoutExtension(path) + "-" + stamp + FileExtension;

        File.Copy(path, Path.Combine(backupDirectory, backupFileName), overwrite: true);

        PruneOldBackups(backupDirectory, Path.GetFileNameWithoutExtension(path));
    }

    private static void PruneOldBackups(string backupDirectory, string projectFileStem)
    {
        var excess = Directory.EnumerateFiles(backupDirectory, projectFileStem + "-*" + FileExtension)
            .OrderByDescending(file => file, StringComparer.Ordinal)
            .Skip(MaxBackupCount);

        foreach (string file in excess)
        {
            File.Delete(file);
        }
    }
}
