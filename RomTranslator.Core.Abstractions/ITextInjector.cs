// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;

namespace RomTranslator.Core.Abstractions;

/// <summary>Résultat de la réinjection du texte traduit dans une image ROM.</summary>
/// <param name="OutputRomPath">Chemin de l'image ROM générée.</param>
/// <param name="Messages">Messages d'information ou d'avertissement (déjà traduits) produits pendant l'injection.</param>
public sealed record TextInjectionResult(string OutputRomPath, IReadOnlyList<string> Messages);

/// <summary>Réinjecte le texte traduit dans une image ROM, en respectant la table de caractères d'origine.</summary>
public interface ITextInjector
{
    /// <summary>
    /// Réécrit les entrées traduites dans une copie de l'image ROM source, en relocalisant les pointeurs
    /// affectés si la longueur d'une chaîne change.
    /// </summary>
    /// <param name="sourceRomPath">Chemin de l'image ROM d'origine (jamais modifiée).</param>
    /// <param name="outputRomPath">Chemin de l'image ROM à générer.</param>
    /// <param name="entries">Entrées de traduction à réinjecter.</param>
    /// <param name="characterTable">Table de caractères à utiliser pour encoder le texte traduit.</param>
    TextInjectionResult Inject(
        string sourceRomPath,
        string outputRomPath,
        IReadOnlyList<ITranslationEntry> entries,
        ICharacterTable characterTable);
}
