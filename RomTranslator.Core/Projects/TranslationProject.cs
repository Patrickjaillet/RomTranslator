// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;

namespace RomTranslator.Core.Projects;

/// <summary>
/// Un projet de traduction : la console et la ROM source ciblées, la table de caractères utilisée et les
/// entrées de texte extraites. Enregistré dans un fichier portable au format <c>.rtproj</c> (JSON versionné).
/// </summary>
public sealed class TranslationProject
{
    /// <summary>Version courante du format de fichier projet, pour les migrations futures.</summary>
    public const int CurrentSchemaVersion = 1;

    private List<TranslationEntry> _entries = new();

    /// <summary>Version du format du fichier, pour les migrations futures.</summary>
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    /// <summary>Nom du projet, choisi par l'utilisateur à la création.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Identifiant du module console ciblé (voir <c>IConsoleModule.Id</c>).</summary>
    public string ConsoleId { get; set; } = string.Empty;

    /// <summary>
    /// Chemin de l'image ROM source, relatif au fichier projet lorsque c'est possible (portabilité :
    /// le projet et sa ROM peuvent être déplacés ensemble sans casser la référence).
    /// </summary>
    public string RomPath { get; set; } = string.Empty;

    /// <summary>Nom du fichier de table de caractères utilisé par ce projet, ou <see langword="null" /> si aucun n'est choisi.</summary>
    public string? CharacterTableName { get; set; }

    /// <summary>Date de création du projet (UTC).</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Date de dernière modification enregistrée (UTC).</summary>
    public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Entrées de traduction du projet (jamais <see langword="null" />).</summary>
    public List<TranslationEntry> Entries
    {
        get => _entries;
        set => _entries = value ?? new List<TranslationEntry>();
    }
}
