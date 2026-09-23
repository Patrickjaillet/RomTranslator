// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.App.ViewModels;

/// <summary>Filtre de statut appliqué à la liste des entrées de l'éditeur de traduction.</summary>
public enum TranslationStatusFilter
{
    /// <summary>Toutes les entrées, quel que soit leur statut.</summary>
    All = 0,

    /// <summary>Uniquement les entrées non traduites.</summary>
    NotTranslated = 1,

    /// <summary>Uniquement les entrées en cours de traduction.</summary>
    InProgress = 2,

    /// <summary>Uniquement les entrées traduites (relues ou non).</summary>
    Translated = 3,

    /// <summary>Uniquement les entrées validées.</summary>
    Validated = 4,

    /// <summary>Uniquement les entrées dont la traduction dépasse la limite de longueur.</summary>
    ExceedsLengthLimit = 5,
}
