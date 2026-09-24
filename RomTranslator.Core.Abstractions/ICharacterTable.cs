// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;

namespace RomTranslator.Core.Abstractions;

/// <summary>
/// Table de correspondance entre octets (ou séquences d'octets, pour les schémas DTE/MTE) et caractères
/// affichables. Propre à une console, parfois même à un jeu.
/// </summary>
public interface ICharacterTable
{
    /// <summary>Nom de la table, affiché dans l'interface (par exemple le nom du jeu ou de l'encodage).</summary>
    string Name { get; }

    /// <summary>
    /// Décode une séquence d'octets en texte, en consommant autant d'octets que nécessaire (schémas DTE/MTE compris).
    /// </summary>
    /// <param name="bytes">Octets à décoder.</param>
    /// <returns>Texte décodé.</returns>
    string Decode(IReadOnlyList<byte> bytes);

    /// <summary>
    /// Tente de décoder une séquence d'octets, sans lever d'exception en cas d'échec. À préférer à
    /// <see cref="Decode" /> pour sonder la validité d'une séquence à haute fréquence (par exemple un
    /// balayage heuristique octet par octet) : une exception .NET a un coût d'exécution significatif,
    /// inadapté à un chemin d'échec potentiellement emprunté des millions de fois sur une grande ROM.
    /// </summary>
    /// <param name="bytes">Octets à décoder.</param>
    /// <param name="text">Texte décodé si la méthode retourne <see langword="true" /> ; <see langword="null" /> sinon.</param>
    /// <returns><see langword="true" /> si la séquence entière a pu être décodée.</returns>
    bool TryDecode(IReadOnlyList<byte> bytes, out string? text);

    /// <summary>Encode du texte en la séquence d'octets correspondante, selon cette table.</summary>
    /// <param name="text">Texte à encoder.</param>
    /// <exception cref="System.ArgumentException">Un caractère du texte n'a pas de correspondance dans la table.</exception>
    IReadOnlyList<byte> Encode(string text);

    /// <summary>
    /// Vérifie la cohérence de la table : pas de séquence d'octets associée à deux caractères différents,
    /// pas de caractère associé à deux séquences différentes.
    /// </summary>
    /// <returns>La liste des incohérences détectées ; vide si la table est cohérente.</returns>
    IReadOnlyList<string> Validate();
}
