// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Editing;
using RomTranslator.Core.Localization;
using RomTranslator.Core.Projects;

namespace RomTranslator.App.ViewModels;

/// <summary>
/// Modèle de vue d'une entrée de traduction. Les modifications de <see cref="TranslatedText" /> et
/// <see cref="Status" /> passent par la pile d'annulation/rétablissement du projet.
/// </summary>
public sealed class TranslationEntryViewModel : ObservableObject
{
    private static readonly CompositeFormat OccurrenceCountLabelFormat = CompositeFormat.Parse(Strings.Editor_OccurrenceCountLabel);
    private static readonly CompositeFormat LengthLimitLabelFormat = CompositeFormat.Parse(Strings.Editor_LengthLimitLabel);
    private static readonly CompositeFormat LengthLimitExceededFormat = CompositeFormat.Parse(Strings.Editor_LengthLimitExceeded);

    private readonly TranslationEntry _entry;
    private readonly UndoRedoStack _undoRedo;
    private readonly ITranslationLengthPolicy? _lengthPolicy;
    private readonly ICharacterTable? _characterTable;

    /// <summary>Initialise le modèle de vue d'une entrée.</summary>
    /// <param name="entry">Entrée enveloppée.</param>
    /// <param name="undoRedo">Pile d'annulation/rétablissement du projet.</param>
    /// <param name="lengthPolicy">Contrainte de longueur du module console, ou <see langword="null" /> si aucune n'est définie.</param>
    /// <param name="characterTable">Table de caractères du projet, requise si <paramref name="lengthPolicy" /> est fourni.</param>
    public TranslationEntryViewModel(
        TranslationEntry entry,
        UndoRedoStack undoRedo,
        ITranslationLengthPolicy? lengthPolicy = null,
        ICharacterTable? characterTable = null)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(undoRedo);

        _entry = entry;
        _undoRedo = undoRedo;
        _lengthPolicy = lengthPolicy;
        _characterTable = characterTable;
    }

    /// <summary>Entrée de traduction enveloppée.</summary>
    public TranslationEntry Entry => _entry;

    /// <summary>Identifiant unique de l'entrée.</summary>
    public string Id => _entry.Id;

    /// <summary>Texte source, en lecture seule.</summary>
    public string SourceText => _entry.SourceText;

    /// <summary>Contexte libre, ou <see langword="null" />.</summary>
    public string? Context => _entry.Context;

    /// <summary>Décalage de l'entrée dans la ROM d'origine, affiché en hexadécimal.</summary>
    public string OffsetDisplay => "0x" + _entry.Offset.ToString("X", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>Nombre d'occurrences identiques regroupées sous cette entrée.</summary>
    public int OccurrenceCount => _entry.OccurrenceCount;

    /// <summary>Texte « N occurrence(s) regroupée(s) » affiché sous le panneau d'édition.</summary>
    public string OccurrenceCountDisplayText =>
        string.Format(CultureInfo.CurrentCulture, OccurrenceCountLabelFormat, _entry.OccurrenceCount);

    /// <summary>Texte traduit. Chaque changement est empilé pour annulation/rétablissement.</summary>
    public string TranslatedText
    {
        get => _entry.TranslatedText;
        set
        {
            if (string.Equals(_entry.TranslatedText, value, StringComparison.Ordinal))
            {
                return;
            }

            _undoRedo.Execute(new EditTranslatedTextCommand(_entry, value));
            OnPropertyChanged();
            OnPropertyChanged(nameof(LengthCheck));
            OnPropertyChanged(nameof(ExceedsLengthLimit));
            OnPropertyChanged(nameof(LengthLimitDisplayText));
            OnPropertyChanged(nameof(LengthLimitExceededMessage));
            OnPropertyChanged(nameof(CharacterTablePreviewText));
        }
    }

    /// <summary>Statut de traduction. Chaque changement est empilé pour annulation/rétablissement.</summary>
    public TranslationStatus Status
    {
        get => _entry.Status;
        set
        {
            if (_entry.Status == value)
            {
                return;
            }

            _undoRedo.Execute(new EditStatusCommand(_entry, value));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Contrôle de longueur courant (limite et longueur encodée), ou <see langword="null" /> si le module
    /// console ne définit pas de contrainte de longueur pour ce projet.
    /// </summary>
    public TranslationLengthCheck? LengthCheck =>
        _lengthPolicy is not null && _characterTable is not null
            ? _lengthPolicy.Check(_entry, _characterTable)
            : null;

    /// <summary>Indique si le texte traduit dépasse la limite de longueur, quand une contrainte est définie.</summary>
    public bool ExceedsLengthLimit => LengthCheck?.ExceedsLimit ?? false;

    /// <summary>Texte « N / limite octets » affiché sous la zone de traduction, ou <see langword="null" /> si aucune contrainte n'est définie.</summary>
    public string? LengthLimitDisplayText => LengthCheck is { } check
        ? string.Format(CultureInfo.CurrentCulture, LengthLimitLabelFormat, check.EncodedLength, check.Limit)
        : null;

    /// <summary>Message d'alerte affiché quand la traduction dépasse la limite, ou <see langword="null" /> sinon.</summary>
    public string? LengthLimitExceededMessage => LengthCheck is { ExceedsLimit: true } check
        ? string.Format(CultureInfo.CurrentCulture, LengthLimitExceededFormat, check.EncodedLength, check.Limit)
        : null;

    /// <summary>
    /// Aperçu du texte traduit tel qu'il sera réellement affiché en jeu, obtenu en encodant puis décodant la
    /// traduction avec la table de caractères du projet (un aller-retour identique au texte saisi signifie que
    /// chaque caractère est représentable) ; <see langword="null" /> si le module console ne fournit pas de
    /// table de caractères, ou si aucun rendu en jeu n'est simulé (voir <see cref="Strings.Editor_CharacterTablePreview_UnsupportedCharacter" />
    /// pour le message affiché en cas de caractère non représentable).
    /// </summary>
    public string? CharacterTablePreviewText
    {
        get
        {
            if (_characterTable is null)
            {
                return null;
            }

            try
            {
                return _characterTable.Decode(_characterTable.Encode(_entry.TranslatedText));
            }
            catch (ArgumentException)
            {
                return Strings.Editor_CharacterTablePreview_UnsupportedCharacter;
            }
        }
    }

    /// <summary>Notifie qu'une propriété affichée a pu changer à la suite d'une annulation/un rétablissement externe.</summary>
    public void RefreshFromEntry()
    {
        OnPropertyChanged(nameof(TranslatedText));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(LengthCheck));
        OnPropertyChanged(nameof(ExceedsLengthLimit));
        OnPropertyChanged(nameof(LengthLimitDisplayText));
        OnPropertyChanged(nameof(LengthLimitExceededMessage));
        OnPropertyChanged(nameof(CharacterTablePreviewText));
    }
}
