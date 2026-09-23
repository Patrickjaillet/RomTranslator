// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;

namespace RomTranslator.Core.Portability;

/// <summary>
/// Emplacements des données de l'application. Tout est situé sous un dossier racine unique (le dossier
/// de l'application) : configuration, projets de traduction et journaux. Aucune donnée n'est jamais
/// écrite dans un dossier utilisateur de Windows ni dans la base de registre, de sorte qu'un simple
/// déplacement du dossier de l'application déplace aussi toutes ses données.
/// </summary>
public sealed class PortableLocations
{
    /// <summary>Nom du sous-dossier de configuration.</summary>
    public const string ConfigDirectoryName = "config";

    /// <summary>Nom du sous-dossier des projets de traduction.</summary>
    public const string ProjectsDirectoryName = "projects";

    /// <summary>Nom du sous-dossier des journaux.</summary>
    public const string LogsDirectoryName = "logs";

    /// <summary>Nom du fichier de paramètres de l'application.</summary>
    public const string SettingsFileName = "settings.json";

    /// <summary>Initialise les emplacements à partir d'un dossier racine.</summary>
    /// <param name="rootDirectory">Dossier racine (chemin absolu ou relatif au répertoire courant).</param>
    public PortableLocations(string rootDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);

        RootDirectory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootDirectory));
    }

    /// <summary>Dossier racine (chemin absolu, sans séparateur final).</summary>
    public string RootDirectory { get; }

    /// <summary>Dossier de configuration.</summary>
    public string ConfigDirectory => Path.Combine(RootDirectory, ConfigDirectoryName);

    /// <summary>Dossier des projets de traduction.</summary>
    public string ProjectsDirectory => Path.Combine(RootDirectory, ProjectsDirectoryName);

    /// <summary>Dossier des journaux.</summary>
    public string LogsDirectory => Path.Combine(RootDirectory, LogsDirectoryName);

    /// <summary>Chemin du fichier de paramètres.</summary>
    public string SettingsFilePath => Path.Combine(ConfigDirectory, SettingsFileName);

    /// <summary>Emplacements situés dans le dossier de l'application en cours d'exécution.</summary>
    public static PortableLocations FromApplicationBase()
    {
        return new PortableLocations(AppContext.BaseDirectory);
    }

    /// <summary>Crée les dossiers de configuration, de projets et de journaux s'ils n'existent pas.</summary>
    /// <exception cref="IOException">Le dossier racine n'est pas accessible en écriture.</exception>
    public void EnsureDirectories()
    {
        Directory.CreateDirectory(ConfigDirectory);
        Directory.CreateDirectory(ProjectsDirectory);
        Directory.CreateDirectory(LogsDirectory);
    }

    /// <summary>Indique si un chemin est situé dans le dossier racine (ou est le dossier racine lui-même).</summary>
    public bool IsInsideRoot(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string fullPath = Path.GetFullPath(path);
        if (fullPath.Equals(RootDirectory, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string prefix = RootDirectory.EndsWith(Path.DirectorySeparatorChar)
            ? RootDirectory
            : RootDirectory + Path.DirectorySeparatorChar;

        return fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Convertit un chemin relatif en chemin absolu, en garantissant qu'il reste sous la racine.</summary>
    /// <param name="relativePath">Chemin relatif au dossier racine.</param>
    /// <exception cref="ArgumentException">Le chemin est absolu ou sort du dossier racine.</exception>
    public string Resolve(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        if (Path.IsPathRooted(relativePath))
        {
            throw new ArgumentException("Un chemin relatif au dossier de l'application est attendu.", nameof(relativePath));
        }

        string fullPath = Path.GetFullPath(Path.Combine(RootDirectory, relativePath));
        if (!IsInsideRoot(fullPath))
        {
            throw new ArgumentException("Le chemin sort du dossier de l'application.", nameof(relativePath));
        }

        return fullPath;
    }
}
