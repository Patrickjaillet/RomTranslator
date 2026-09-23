// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Text.Json.Serialization;
using RomTranslator.Core.Abstractions;

namespace RomTranslator.Core.Projects;

/// <summary>Implémentation générique d'une entrée de traduction, indépendante de toute console.</summary>
public sealed class TranslationEntry : ITranslationEntry
{
    /// <summary>Initialise une entrée de traduction.</summary>
    /// <param name="id">Identifiant unique et stable de l'entrée au sein du projet.</param>
    /// <param name="sourceText">Texte source, tel qu'extrait de la ROM.</param>
    /// <param name="offset">Décalage de l'entrée dans la ROM d'origine.</param>
    /// <param name="occurrenceCount">Nombre d'occurrences identiques regroupées sous cette entrée.</param>
    /// <param name="context">Contexte libre aidant à la traduction, ou <see langword="null" />.</param>
    /// <param name="translatedText">Texte traduit, ou chaîne vide si non traduit.</param>
    /// <param name="status">Statut de traduction.</param>
    [JsonConstructor]
    public TranslationEntry(
        string id,
        string sourceText,
        long offset,
        int occurrenceCount = 1,
        string? context = null,
        string translatedText = "",
        TranslationStatus status = TranslationStatus.NotTranslated)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(sourceText);
        ArgumentNullException.ThrowIfNull(translatedText);
        if (occurrenceCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(occurrenceCount), occurrenceCount, "Le nombre d'occurrences doit être au moins 1.");
        }

        Id = id;
        SourceText = sourceText;
        Offset = offset;
        OccurrenceCount = occurrenceCount;
        Context = context;
        TranslatedText = translatedText;
        Status = status;
    }

    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public string SourceText { get; }

    /// <inheritdoc />
    public string TranslatedText { get; set; }

    /// <inheritdoc />
    public TranslationStatus Status { get; set; }

    /// <inheritdoc />
    public string? Context { get; }

    /// <inheritdoc />
    public long Offset { get; }

    /// <inheritdoc />
    public int OccurrenceCount { get; }
}
