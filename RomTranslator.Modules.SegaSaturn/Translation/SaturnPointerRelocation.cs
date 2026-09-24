// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using RomTranslator.Modules.SegaSaturn.TextExtraction;

namespace RomTranslator.Modules.SegaSaturn.Translation;

/// <summary>
/// Décrit comment reloger les chaînes traduites trop longues pour tenir à la place de leur texte source :
/// l'encodage et l'emplacement des pointeurs à mettre à jour, et une zone de réserve inutilisée de l'image
/// disque où écrire les chaînes relogées. Cette zone doit être identifiée manuellement pour le jeu ciblé
/// (secteur de bourrage, fin de fichier inutilisée...) : <see cref="SaturnTextInjector" /> ne modifie jamais
/// la taille de l'image ni la structure du système de fichiers pour lui en trouver une automatiquement.
/// </summary>
/// <param name="PointerEncoding">Encodage des pointeurs du jeu ciblé.</param>
/// <param name="PointerOffsetsByStringOffset">
/// Pour chaque décalage de chaîne source connu de l'extraction, la liste des décalages des pointeurs de la
/// ROM qui la désignent. Une chaîne sans pointeur connu (absente de ce dictionnaire) n'est jamais relogée :
/// si sa traduction encodée est plus longue que l'originale, l'injection échoue pour cette entrée plutôt que
/// d'écrire une chaîne qu'aucun pointeur ne pourrait retrouver.
/// </param>
/// <param name="FreeSpaceOffset">Décalage logique du premier octet de la zone de réserve.</param>
/// <param name="FreeSpaceLength">Longueur, en octets, de la zone de réserve.</param>
public sealed record SaturnPointerRelocation(
    PointerEncoding PointerEncoding,
    IReadOnlyDictionary<long, IReadOnlyList<long>> PointerOffsetsByStringOffset,
    long FreeSpaceOffset,
    long FreeSpaceLength)
{
    /// <summary>Recherche les pointeurs connus vers un décalage de chaîne source donné.</summary>
    /// <param name="stringOffset">Décalage de la chaîne source dans la ROM.</param>
    /// <returns>La liste des décalages de pointeurs, ou une liste vide si aucun n'est connu pour ce décalage.</returns>
    public IReadOnlyList<long> FindPointerOffsets(long stringOffset)
    {
        return PointerOffsetsByStringOffset.TryGetValue(stringOffset, out IReadOnlyList<long>? offsets)
            ? offsets
            : Array.Empty<long>();
    }
}
