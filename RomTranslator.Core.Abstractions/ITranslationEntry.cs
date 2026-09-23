// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.Core.Abstractions;

/// <summary>Statut de traduction d'une entrée de texte.</summary>
public enum TranslationStatus
{
    /// <summary>Le texte source n'a pas encore été traduit.</summary>
    NotTranslated = 0,

    /// <summary>La traduction est en cours de rédaction.</summary>
    InProgress = 1,

    /// <summary>La traduction est terminée mais pas encore relue.</summary>
    Translated = 2,

    /// <summary>La traduction a été relue et validée.</summary>
    Validated = 3,
}

/// <summary>
/// Une entrée de texte extraite d'une ROM : texte source, traduction, statut et position d'origine.
/// </summary>
public interface ITranslationEntry
{
    /// <summary>Identifiant unique et stable de l'entrée au sein du projet.</summary>
    string Id { get; }

    /// <summary>Texte source, tel qu'extrait de la ROM.</summary>
    string SourceText { get; }

    /// <summary>Texte traduit, ou chaîne vide si non traduit.</summary>
    string TranslatedText { get; set; }

    /// <summary>Statut de traduction courant.</summary>
    TranslationStatus Status { get; set; }

    /// <summary>Contexte libre aidant à la traduction (personnage, scène, contrainte narrative...), ou <see langword="null" />.</summary>
    string? Context { get; }

    /// <summary>Décalage (offset) de l'entrée dans la ROM d'origine.</summary>
    long Offset { get; }

    /// <summary>Nombre d'occurrences identiques regroupées sous cette entrée (au moins 1).</summary>
    int OccurrenceCount { get; }
}
