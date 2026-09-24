// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;

namespace RomTranslator.Core.Projects;

/// <summary>Une incohérence entre le glossaire d'un projet et une entrée déjà traduite.</summary>
/// <param name="Entry">Entrée dont la traduction diverge du glossaire.</param>
/// <param name="GlossaryEntry">Entrée de glossaire concernée.</param>
public sealed record GlossaryInconsistency(TranslationEntry Entry, GlossaryEntry GlossaryEntry);

/// <summary>
/// Vérifie qu'une entrée traduite contenant un terme du glossaire utilise bien la traduction retenue pour
/// ce terme, pour aider à repérer les incohérences terminologiques au fil de la traduction.
/// </summary>
public static class GlossaryConsistencyChecker
{
    /// <summary>
    /// Recherche les entrées dont le texte source contient un terme du glossaire, mais dont le texte
    /// traduit ne contient pas la traduction retenue pour ce terme (uniquement pour les entrées déjà
    /// traduites : une entrée non traduite n'est jamais signalée).
    /// </summary>
    /// <param name="entries">Entrées de traduction du projet.</param>
    /// <param name="glossary">Glossaire du projet.</param>
    public static IReadOnlyList<GlossaryInconsistency> FindInconsistencies(
        IReadOnlyList<TranslationEntry> entries, IReadOnlyList<GlossaryEntry> glossary)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(glossary);

        List<GlossaryInconsistency> inconsistencies = new();

        foreach (TranslationEntry entry in entries)
        {
            if (string.IsNullOrEmpty(entry.TranslatedText))
            {
                continue;
            }

            foreach (GlossaryEntry glossaryEntry in glossary)
            {
                bool sourceContainsTerm = entry.SourceText.Contains(glossaryEntry.SourceTerm, StringComparison.OrdinalIgnoreCase);
                bool translationUsesRetainedTerm = entry.TranslatedText.Contains(glossaryEntry.TargetTerm, StringComparison.OrdinalIgnoreCase);

                if (sourceContainsTerm && !translationUsesRetainedTerm)
                {
                    inconsistencies.Add(new GlossaryInconsistency(entry, glossaryEntry));
                }
            }
        }

        return inconsistencies;
    }
}
