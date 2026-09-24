// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;

namespace RomTranslator.Modules.SegaSaturn.TextExtraction;

/// <summary>Paramètres du balayage heuristique de <see cref="SaturnTextExtractor.ExtractAutomatically" />.</summary>
/// <param name="MinimumLength">
/// Longueur minimale (en caractères décodés) d'une chaîne candidate pour être retenue. Une valeur trop
/// basse produit beaucoup de faux positifs sur des données binaires ; une valeur trop haute manque les
/// chaînes courtes (menus, noms d'objets).
/// </param>
/// <param name="MinimumScore">
/// Score de vraisemblance minimal (voir <see cref="CandidateStringScorer.Score" />) pour retenir une chaîne
/// candidate, de 0 (tout accepter) à 1 (n'accepter que des catégories entièrement plausibles).
/// </param>
public sealed record TextScanOptions(int MinimumLength, double MinimumScore)
{
    /// <summary>Valeurs par défaut, adaptées à un premier balayage exploratoire.</summary>
    public static TextScanOptions Default { get; } = new(MinimumLength: 4, MinimumScore: 0.8);

    /// <summary>Valide que les paramètres sont dans des plages exploitables.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="MinimumLength" /> est inférieur à 1, ou <see cref="MinimumScore" /> hors de [0, 1].</exception>
    public void Validate()
    {
        if (MinimumLength < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(MinimumLength), MinimumLength, "La longueur minimale doit être au moins 1.");
        }

        if (MinimumScore is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(MinimumScore), MinimumScore, "Le score minimal doit être compris entre 0 et 1.");
        }
    }
}
