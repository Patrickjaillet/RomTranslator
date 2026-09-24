// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using RomTranslator.Core.Editing;

namespace RomTranslator.Core.Projects;

/// <summary>
/// Prépare un remplacement de texte dans l'ensemble des traductions d'un projet, sous la forme d'une seule
/// commande annulable (<see cref="CompositeEditCommand" />) plutôt que d'une modification directe, pour que
/// l'opération reste réversible en une fois par l'utilisateur.
/// </summary>
public static class TranslationFindAndReplace
{
    /// <summary>
    /// Prépare le remplacement de toutes les occurrences de <paramref name="searchText" /> par
    /// <paramref name="replacementText" /> dans le texte traduit des entrées fournies.
    /// </summary>
    /// <param name="entries">Entrées du projet dans lesquelles chercher.</param>
    /// <param name="searchText">Texte recherché (comparaison ordinale, sensible à la casse).</param>
    /// <param name="replacementText">Texte de remplacement.</param>
    /// <returns>
    /// Une commande composite qui applique le remplacement à toutes les entrées concernées, ou
    /// <see langword="null" /> si <paramref name="searchText" /> n'apparaît dans aucune traduction.
    /// </returns>
    public static CompositeEditCommand? PrepareReplaceAll(
        IReadOnlyList<TranslationEntry> entries, string searchText, string replacementText)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText);
        ArgumentNullException.ThrowIfNull(replacementText);

        List<IEditCommand> commands = new();

        foreach (TranslationEntry entry in entries)
        {
            if (!entry.TranslatedText.Contains(searchText, StringComparison.Ordinal))
            {
                continue;
            }

            string replaced = entry.TranslatedText.Replace(searchText, replacementText, StringComparison.Ordinal);
            commands.Add(new EditTranslatedTextCommand(entry, replaced));
        }

        return commands.Count == 0 ? null : new CompositeEditCommand(commands);
    }

    /// <summary>Compte le nombre d'entrées dont le texte traduit contient <paramref name="searchText" />.</summary>
    /// <param name="entries">Entrées du projet dans lesquelles chercher.</param>
    /// <param name="searchText">Texte recherché (comparaison ordinale, sensible à la casse).</param>
    public static int CountMatches(IReadOnlyList<TranslationEntry> entries, string searchText)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText);

        int count = 0;
        foreach (TranslationEntry entry in entries)
        {
            if (entry.TranslatedText.Contains(searchText, StringComparison.Ordinal))
            {
                count++;
            }
        }

        return count;
    }
}
