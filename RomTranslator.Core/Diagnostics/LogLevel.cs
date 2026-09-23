// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.Core.Diagnostics;

/// <summary>Niveau de verbosité d'un message du journal d'activité.</summary>
public enum LogLevel
{
    /// <summary>Détail technique utile au diagnostic, sans intérêt en usage normal.</summary>
    Debug = 0,

    /// <summary>Déroulement normal de l'application (ouverture de projet, extraction terminée, etc.).</summary>
    Information = 1,

    /// <summary>Situation inhabituelle mais non bloquante.</summary>
    Warning = 2,

    /// <summary>Erreur empêchant une opération de se terminer normalement.</summary>
    Error = 3,
}
