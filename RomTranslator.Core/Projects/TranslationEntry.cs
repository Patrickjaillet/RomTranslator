// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using RomTranslator.Core.Abstractions;

namespace RomTranslator.Core.Projects;

/// <summary>Implémentation générique d'une entrée de traduction, indépendante de toute console.</summary>
public sealed class TranslationEntry : ITranslationEntry
{
    /// <summary>Initialise une entrée de traduction.</summary>
    /// <param name="id">Identifiant unique et stable de l'entrée au sein du projet.</param>
    /// <param name="sourceText">Texte source, tel qu'extrait de la ROM.</param>
    /// <param name="offsets">Décalages de chaque occurrence identique du texte source dans la ROM d'origine (au moins un élément).</param>
    /// <param name="context">Contexte libre aidant à la traduction, ou <see langword="null" />.</param>
    /// <param name="translatedText">Texte traduit, ou chaîne vide si non traduit.</param>
    /// <param name="status">Statut de traduction.</param>
    [JsonConstructor]
    public TranslationEntry(
        string id,
        string sourceText,
        IReadOnlyList<long> offsets,
        string? context = null,
        string translatedText = "",
        TranslationStatus status = TranslationStatus.NotTranslated)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(sourceText);
        ArgumentNullException.ThrowIfNull(offsets);
        ArgumentNullException.ThrowIfNull(translatedText);
        if (offsets.Count == 0)
        {
            throw new ArgumentException("Une entrée doit avoir au moins un décalage.", nameof(offsets));
        }

        Id = id;
        SourceText = sourceText;
        Offsets = offsets;
        Context = context;
        TranslatedText = translatedText;
        Status = status;
    }

    /// <summary>Initialise une entrée de traduction à occurrence unique.</summary>
    /// <param name="id">Identifiant unique et stable de l'entrée au sein du projet.</param>
    /// <param name="sourceText">Texte source, tel qu'extrait de la ROM.</param>
    /// <param name="offset">Décalage de l'entrée dans la ROM d'origine.</param>
    /// <param name="context">Contexte libre aidant à la traduction, ou <see langword="null" />.</param>
    /// <param name="translatedText">Texte traduit, ou chaîne vide si non traduit.</param>
    /// <param name="status">Statut de traduction.</param>
    public TranslationEntry(
        string id,
        string sourceText,
        long offset,
        string? context = null,
        string translatedText = "",
        TranslationStatus status = TranslationStatus.NotTranslated)
        : this(id, sourceText, new[] { offset }, context, translatedText, status)
    {
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
    public IReadOnlyList<long> Offsets { get; }

    /// <inheritdoc />
    public long Offset => Offsets[0];

    /// <inheritdoc />
    public int OccurrenceCount => Offsets.Count;
}
