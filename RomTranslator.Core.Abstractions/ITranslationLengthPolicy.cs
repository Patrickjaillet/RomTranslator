// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.Core.Abstractions;

/// <summary>Résultat du contrôle de longueur d'une traduction par rapport à sa contrainte d'espace mémoire.</summary>
/// <param name="Limit">Longueur maximale autorisée, en octets une fois encodée selon la table de caractères du module.</param>
/// <param name="EncodedLength">Longueur réelle du texte traduit, en octets une fois encodée.</param>
public readonly record struct TranslationLengthCheck(int Limit, int EncodedLength)
{
    /// <summary>Indique si le texte traduit dépasse la limite autorisée.</summary>
    public bool ExceedsLimit => EncodedLength > Limit;
}

/// <summary>
/// Calcule la contrainte de longueur d'une entrée de traduction. Propre à chaque module console, car la
/// limite dépend de l'espace mémoire disponible dans la ROM (souvent liée à la longueur du texte source,
/// parfois à un pointeur ou une taille de bloc fixe).
/// </summary>
public interface ITranslationLengthPolicy
{
    /// <summary>Calcule la limite de longueur et la longueur encodée courante d'une entrée.</summary>
    /// <param name="entry">Entrée à contrôler.</param>
    /// <param name="characterTable">Table de caractères utilisée pour encoder le texte traduit.</param>
    TranslationLengthCheck Check(ITranslationEntry entry, ICharacterTable characterTable);
}
