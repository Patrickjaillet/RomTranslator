// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Globalization;

namespace RomTranslator.Modules.SegaSaturn.TextExtraction;

/// <summary>
/// Attribue un score de vraisemblance (0 à 1) à une chaîne candidate décodée durant le balayage
/// heuristique, pour distinguer du texte plausible de données binaires qui se décodent par coïncidence.
/// Le score repose sur la catégorie Unicode de chaque caractère plutôt que sur une fréquence statistique de
/// lettres : aucun corpus de référence de jeux Saturn n'est disponible pour calibrer un modèle plus fin, et
/// une règle par catégorie reste simple à expliquer et à ajuster.
/// </summary>
public static class CandidateStringScorer
{
    /// <summary>Calcule le score de vraisemblance d'une chaîne candidate.</summary>
    /// <param name="text">Texte décodé à évaluer.</param>
    /// <returns>
    /// Un score de 0 (très improbable) à 1 (entièrement composé de catégories plausibles pour du texte de jeu).
    /// Retourne 0 pour une chaîne vide.
    /// </returns>
    public static double Score(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        int plausibleCount = 0;
        foreach (char c in text)
        {
            if (IsPlausibleGameTextCharacter(c))
            {
                plausibleCount++;
            }
        }

        return (double)plausibleCount / text.Length;
    }

    /// <summary>
    /// Indique si un caractère appartient à une catégorie plausible dans du texte de jeu : lettre, chiffre,
    /// espace, ou ponctuation courante (guillemets, points, virgules, tirets, points d'exclamation/interrogation).
    /// Les catégories de contrôle, de symboles rares ou les caractères de remplacement sont exclus.
    /// </summary>
    private static bool IsPlausibleGameTextCharacter(char c)
    {
        if (c == '�')
        {
            return false;
        }

        UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);

        return category switch
        {
            UnicodeCategory.UppercaseLetter => true,
            UnicodeCategory.LowercaseLetter => true,
            UnicodeCategory.TitlecaseLetter => true,
            UnicodeCategory.OtherLetter => true,
            UnicodeCategory.ModifierLetter => true,
            UnicodeCategory.DecimalDigitNumber => true,
            UnicodeCategory.SpaceSeparator => true,
            UnicodeCategory.DashPunctuation => true,
            UnicodeCategory.ConnectorPunctuation => true,
            UnicodeCategory.OpenPunctuation => true,
            UnicodeCategory.ClosePunctuation => true,
            UnicodeCategory.InitialQuotePunctuation => true,
            UnicodeCategory.FinalQuotePunctuation => true,
            UnicodeCategory.OtherPunctuation => true,
            _ => false,
        };
    }
}
