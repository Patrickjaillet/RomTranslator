// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Editing;
using RomTranslator.Core.Localization;
using RomTranslator.Core.Projects;

namespace RomTranslator.App.ViewModels;

/// <summary>
/// Éditeur de traduction générique : liste filtrable des entrées d'un projet, panneau d'édition de
/// l'entrée sélectionnée, recherche, annulation/rétablissement et statistiques de progression. Réutilisable
/// par tout module console : ne connaît que <see cref="TranslationProject" /> et les contrats du Core.
/// </summary>
public sealed class TranslationEditorViewModel : ObservableObject
{
    private readonly TranslationProject _project;
    private readonly UndoRedoStack _undoRedo = new();
    private readonly List<TranslationEntryViewModel> _entries;
    private readonly ICollectionView _entriesView;

    private string _searchText = string.Empty;
    private TranslationStatusFilter _statusFilter = TranslationStatusFilter.All;
    private TranslationEntryViewModel? _selectedEntry;

    /// <summary>Initialise l'éditeur pour un projet chargé.</summary>
    /// <param name="project">Projet dont les entrées sont éditées.</param>
    /// <param name="lengthPolicy">Contrainte de longueur du module console, ou <see langword="null" /> si aucune n'est définie.</param>
    /// <param name="characterTable">Table de caractères du projet, requise si <paramref name="lengthPolicy" /> est fourni.</param>
    public TranslationEditorViewModel(
        TranslationProject project,
        ITranslationLengthPolicy? lengthPolicy = null,
        ICharacterTable? characterTable = null)
    {
        ArgumentNullException.ThrowIfNull(project);

        _project = project;
        _entries = project.Entries
            .Select(entry => new TranslationEntryViewModel(entry, _undoRedo, lengthPolicy, characterTable))
            .ToList();

        _entriesView = CollectionViewSource.GetDefaultView(_entries);
        _entriesView.Filter = FilterEntry;

        _undoRedo.StateChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            UndoCommand.NotifyCanExecuteChanged();
            RedoCommand.NotifyCanExecuteChanged();

            foreach (TranslationEntryViewModel entry in _entries)
            {
                entry.RefreshFromEntry();
            }

            RefreshStatistics();
            _entriesView.Refresh();
        };

        UndoCommand = new RelayCommand(() => _undoRedo.Undo(), () => CanUndo);
        RedoCommand = new RelayCommand(() => _undoRedo.Redo(), () => CanRedo);

        RefreshStatistics();
        SelectedEntry = _entries.FirstOrDefault();
    }

    /// <summary>Vue filtrée des entrées, à lier à la liste de l'interface.</summary>
    public ICollectionView Entries => _entriesView;

    /// <summary>Nombre total d'entrées du projet.</summary>
    public int TotalCount => _entries.Count;

    /// <summary>Nombre d'entrées traduites ou validées.</summary>
    public int TranslatedCount { get; private set; }

    /// <summary>Pourcentage d'entrées traduites ou validées, de 0 à 100.</summary>
    public double TranslatedPercent { get; private set; }

    /// <summary>Texte « N sur M entrées traduites (P %) » affiché sous la barre de progression.</summary>
    public string ProgressSummaryText { get; private set; } = string.Empty;

    /// <summary>Texte de recherche appliqué au texte source et au texte traduit.</summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value ?? string.Empty))
            {
                _entriesView.Refresh();
            }
        }
    }

    /// <summary>Filtre de statut appliqué à la liste.</summary>
    public TranslationStatusFilter StatusFilter
    {
        get => _statusFilter;
        set
        {
            if (SetProperty(ref _statusFilter, value))
            {
                _entriesView.Refresh();
            }
        }
    }

    /// <summary>Entrée actuellement sélectionnée dans le panneau d'édition, ou <see langword="null" />.</summary>
    public TranslationEntryViewModel? SelectedEntry
    {
        get => _selectedEntry;
        set => SetProperty(ref _selectedEntry, value);
    }

    /// <summary>Indique si une annulation est possible.</summary>
    public bool CanUndo => _undoRedo.CanUndo;

    /// <summary>Indique si un rétablissement est possible.</summary>
    public bool CanRedo => _undoRedo.CanRedo;

    /// <summary>Annule la dernière modification.</summary>
    public RelayCommand UndoCommand { get; }

    /// <summary>Rétablit la dernière modification annulée.</summary>
    public RelayCommand RedoCommand { get; }

    /// <summary>Exporte les traductions courantes vers un fichier CSV, pour relecture externe.</summary>
    /// <param name="path">Chemin du fichier CSV à créer.</param>
    public void ExportCsv(string path)
    {
        TranslationCsvExchange.Export(_project.Entries, path);
    }

    /// <summary>
    /// Importe des traductions relues depuis un fichier CSV et rafraîchit l'affichage des entrées modifiées.
    /// </summary>
    /// <param name="path">Chemin du fichier CSV relu.</param>
    /// <returns>Le nombre d'entrées mises à jour.</returns>
    public int ImportCsv(string path)
    {
        int updatedCount = TranslationCsvExchange.Import(_project.Entries, path);

        foreach (TranslationEntryViewModel entry in _entries)
        {
            entry.RefreshFromEntry();
        }

        RefreshStatistics();
        _entriesView.Refresh();

        return updatedCount;
    }

    private bool FilterEntry(object candidate)
    {
        if (candidate is not TranslationEntryViewModel entry)
        {
            return false;
        }

        if (!MatchesStatusFilter(entry))
        {
            return false;
        }

        if (_searchText.Length == 0)
        {
            return true;
        }

        return entry.SourceText.Contains(_searchText, StringComparison.CurrentCultureIgnoreCase)
            || entry.TranslatedText.Contains(_searchText, StringComparison.CurrentCultureIgnoreCase);
    }

    private bool MatchesStatusFilter(TranslationEntryViewModel entry)
    {
        return _statusFilter switch
        {
            TranslationStatusFilter.All => true,
            TranslationStatusFilter.NotTranslated => entry.Status == TranslationStatus.NotTranslated,
            TranslationStatusFilter.InProgress => entry.Status == TranslationStatus.InProgress,
            TranslationStatusFilter.Translated => entry.Status == TranslationStatus.Translated,
            TranslationStatusFilter.Validated => entry.Status == TranslationStatus.Validated,
            TranslationStatusFilter.ExceedsLengthLimit => entry.ExceedsLengthLimit,
            _ => true,
        };
    }

    private void RefreshStatistics()
    {
        TranslatedCount = _entries.Count(entry => entry.Status is TranslationStatus.Translated or TranslationStatus.Validated);
        TranslatedPercent = TotalCount == 0 ? 0 : Math.Round(TranslatedCount * 100.0 / TotalCount, 1);
        ProgressSummaryText = string.Format(
            CultureInfo.CurrentCulture, Strings.Editor_ProgressSummary, TranslatedCount, TotalCount, TranslatedPercent);

        OnPropertyChanged(nameof(TranslatedCount));
        OnPropertyChanged(nameof(TranslatedPercent));
        OnPropertyChanged(nameof(ProgressSummaryText));
    }
}
