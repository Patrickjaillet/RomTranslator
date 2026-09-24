// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.Core.Abstractions;

namespace RomTranslator.Modules.SegaSaturn.Translation;

/// <summary>
/// Contrainte de longueur par défaut pour le module Sega Saturn : une traduction ne doit pas dépasser, une
/// fois encodée selon la table de caractères active, la longueur en octets du texte source. C'est
/// l'hypothèse la plus prudente en l'absence d'information plus précise sur un jeu donné (aucun octet
/// mémoire supplémentaire alloué par rapport à l'original), cohérente avec le fait que la Phase 5.1 n'a
/// identifié aucune convention de dimensionnement commune à tous les jeux Saturn.
/// </summary>
/// <remarks>
/// Un jeu dont le format de données autorise davantage d'espace (blocs de taille fixe plus grands que le
/// texte, pointeurs relogeables sans contrainte de taille) peut fournir sa propre implémentation
/// d'<see cref="ITranslationLengthPolicy" /> avec une règle plus permissive ; celle-ci reste le choix par
/// défaut le plus sûr tant qu'un jeu précis n'a pas été étudié.
/// </remarks>
public sealed class SaturnTranslationLengthPolicy : ITranslationLengthPolicy
{
    /// <inheritdoc />
    /// <remarks>
    /// Si le texte traduit contient un caractère absent de <paramref name="characterTable" /> (par exemple
    /// un accent français avant qu'une police modifiée ne soit disponible pour le jeu ciblé, voir
    /// <c>docs/technique/SegaSaturn.md</c> § 6), la longueur encodée ne peut pas être calculée : ce cas
    /// retourne une longueur conventionnelle strictement supérieure à la limite (pour signaler le
    /// dépassement à l'affichage) plutôt que de lever une exception, l'éditeur devant rester utilisable
    /// pendant la frappe d'un caractère pas encore pris en charge par la table.
    /// </remarks>
    public TranslationLengthCheck Check(ITranslationEntry entry, ICharacterTable characterTable)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(characterTable);

        int limit = characterTable.Encode(entry.SourceText).Count;
        int encodedLength = TryEncodeLength(entry.TranslatedText, characterTable, limit);

        return new TranslationLengthCheck(limit, encodedLength);
    }

    private static int TryEncodeLength(string text, ICharacterTable characterTable, int limit)
    {
        try
        {
            return characterTable.Encode(text).Count;
        }
        catch (ArgumentException)
        {
            return limit + 1;
        }
    }
}
