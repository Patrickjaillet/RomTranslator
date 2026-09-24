// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Text.Json.Serialization;

namespace RomTranslator.Core.Projects;

/// <summary>
/// Une entrée du glossaire d'un projet : un terme source récurrent (nom de personnage, objet, lieu) et la
/// traduction à lui appliquer de façon cohérente dans tout le projet.
/// </summary>
public sealed class GlossaryEntry
{
    /// <summary>Initialise une entrée de glossaire.</summary>
    /// <param name="sourceTerm">Terme source (tel qu'il apparaît dans le texte extrait).</param>
    /// <param name="targetTerm">Traduction à appliquer de façon cohérente pour ce terme.</param>
    /// <param name="note">Remarque libre (contexte, genre grammatical, ambiguïté à éviter...), ou <see langword="null" />.</param>
    [JsonConstructor]
    public GlossaryEntry(string sourceTerm, string targetTerm, string? note = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceTerm);
        ArgumentNullException.ThrowIfNull(targetTerm);

        SourceTerm = sourceTerm;
        TargetTerm = targetTerm;
        Note = note;
    }

    /// <summary>Terme source.</summary>
    public string SourceTerm { get; }

    /// <summary>Traduction retenue pour ce terme.</summary>
    public string TargetTerm { get; set; }

    /// <summary>Remarque libre, ou <see langword="null" />.</summary>
    public string? Note { get; set; }
}
