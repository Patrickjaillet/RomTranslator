// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;

namespace RomTranslator.Core.Abstractions;

/// <summary>Extrait les blocs de texte d'une image ROM, selon une table de caractères donnée.</summary>
public interface ITextExtractor
{
    /// <summary>
    /// Recherche automatiquement les blocs de texte de l'image ROM, selon la table de caractères fournie
    /// (balayage heuristique).
    /// </summary>
    /// <param name="romPath">Chemin de l'image ROM.</param>
    /// <param name="characterTable">Table de caractères à utiliser pour reconnaître le texte.</param>
    IReadOnlyList<ITranslationEntry> ExtractAutomatically(string romPath, ICharacterTable characterTable);

    /// <summary>Extrait le texte d'une plage d'adresses définie manuellement.</summary>
    /// <param name="romPath">Chemin de l'image ROM.</param>
    /// <param name="characterTable">Table de caractères à utiliser pour décoder le texte.</param>
    /// <param name="startOffset">Décalage de début de la plage (inclus).</param>
    /// <param name="endOffset">Décalage de fin de la plage (exclu).</param>
    IReadOnlyList<ITranslationEntry> ExtractRange(string romPath, ICharacterTable characterTable, long startOffset, long endOffset);
}
