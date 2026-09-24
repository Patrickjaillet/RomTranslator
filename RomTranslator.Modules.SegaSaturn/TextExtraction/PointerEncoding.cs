// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.Modules.SegaSaturn.TextExtraction;

/// <summary>
/// Décrit comment un jeu encode ses pointeurs vers des chaînes de texte. Aucune convention n'est commune à
/// tous les jeux Saturn (voir <c>docs/technique/SegaSaturn.md</c> § 5) : ces paramètres doivent être établis
/// au cas par cas par rétro-ingénierie du jeu ciblé, puis renseignés explicitement.
/// </summary>
/// <param name="ByteWidth">Taille d'un pointeur, en octets (généralement 2 ou 4).</param>
/// <param name="IsBigEndian">Indique si les octets du pointeur sont stockés en boutisme big-endian (natif SH-2).</param>
/// <param name="IsRelative">
/// Indique si le pointeur est relatif à <see cref="BaseOffset" /> (une valeur ajoutée à la base donne l'offset
/// réel de la chaîne) ou absolu (la valeur lue est directement l'offset réel de la chaîne).
/// </param>
/// <param name="BaseOffset">
/// Décalage de base utilisé pour résoudre un pointeur relatif. Sans effet si <see cref="IsRelative" /> est
/// <see langword="false" />.
/// </param>
public sealed record PointerEncoding(int ByteWidth, bool IsBigEndian, bool IsRelative, long BaseOffset)
{
    /// <summary>Résout la valeur lue à l'emplacement d'un pointeur vers l'offset réel de la chaîne qu'il cible.</summary>
    /// <param name="rawValue">Valeur brute lue à l'emplacement du pointeur.</param>
    public long ResolveTargetOffset(long rawValue)
    {
        return IsRelative ? BaseOffset + rawValue : rawValue;
    }

    /// <summary>Calcule la valeur brute à écrire à l'emplacement d'un pointeur pour qu'il cible un offset donné.</summary>
    /// <param name="targetOffset">Offset réel visé par le pointeur.</param>
    public long ComputeRawValue(long targetOffset)
    {
        return IsRelative ? targetOffset - BaseOffset : targetOffset;
    }
}
