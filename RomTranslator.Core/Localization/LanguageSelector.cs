// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;

namespace RomTranslator.Core.Localization;

/// <summary>Détermine la culture d'interface active au démarrage.</summary>
public static class LanguageSelector
{
    /// <summary>Codes de langue pris en charge par l'interface (culture neutre par défaut : français).</summary>
    public static readonly string[] SupportedLanguageCodes = { "fr", "en" };

    /// <summary>
    /// Résout la culture à utiliser : la langue enregistrée dans les paramètres si elle est prise en charge,
    /// sinon la langue du système si elle est prise en charge, sinon le français.
    /// </summary>
    /// <param name="savedLanguageCode">Code de langue enregistré dans les paramètres, ou <see langword="null" /> si aucun choix explicite.</param>
    /// <param name="systemLanguageCode">Code de langue du système (culture UI courante).</param>
    public static CultureInfo Resolve(string? savedLanguageCode, string systemLanguageCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(systemLanguageCode);

        string code = IsSupported(savedLanguageCode) ? savedLanguageCode! : IsSupported(systemLanguageCode) ? systemLanguageCode : "fr";

        return CultureInfo.GetCultureInfo(code);
    }

    /// <summary>Indique si un code de langue est pris en charge par l'interface.</summary>
    public static bool IsSupported(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return false;
        }

        foreach (string supported in SupportedLanguageCodes)
        {
            if (string.Equals(supported, languageCode, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
