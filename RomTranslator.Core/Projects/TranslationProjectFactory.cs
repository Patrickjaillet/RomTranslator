// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;

namespace RomTranslator.Core.Projects;

/// <summary>Crée de nouveaux projets de traduction.</summary>
public static class TranslationProjectFactory
{
    /// <summary>
    /// Crée un nouveau projet vide pour une console et une image ROM source données.
    /// </summary>
    /// <param name="name">Nom du projet, choisi par l'utilisateur.</param>
    /// <param name="consoleId">Identifiant du module console ciblé (voir <c>IConsoleModule.Id</c>).</param>
    /// <param name="romPath">Chemin de l'image ROM source.</param>
    public static TranslationProject Create(string name, string consoleId, string romPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(consoleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);

        DateTimeOffset now = DateTimeOffset.UtcNow;

        return new TranslationProject
        {
            Name = name,
            ConsoleId = consoleId,
            RomPath = romPath,
            CreatedAt = now,
            ModifiedAt = now,
        };
    }
}
