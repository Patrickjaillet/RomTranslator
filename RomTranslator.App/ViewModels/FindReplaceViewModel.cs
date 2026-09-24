// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomTranslator.Core.Editing;
using RomTranslator.Core.Localization;
using RomTranslator.Core.Projects;

namespace RomTranslator.App.ViewModels;

/// <summary>
/// Recherche et remplacement dans l'ensemble des traductions d'un projet. Le remplacement est appliqué par
/// la pile d'annulation/rétablissement du projet, comme une seule étape annulable en une fois.
/// </summary>
public sealed class FindReplaceViewModel : ObservableObject
{
    private static readonly CompositeFormat MatchCountFormat = CompositeFormat.Parse(Strings.FindReplace_MatchCount);
    private static readonly CompositeFormat NoMatchFormat = CompositeFormat.Parse(Strings.FindReplace_NoMatch);
    private static readonly CompositeFormat ReplacedCountFormat = CompositeFormat.Parse(Strings.FindReplace_ReplacedCount);

    private readonly IReadOnlyList<TranslationEntry> _entries;
    private readonly UndoRedoStack _undoRedo;
    private readonly Action _onReplaced;

    private string _searchText = string.Empty;
    private string _replacementText = string.Empty;
    private int _matchCount;
    private string _resultMessage = string.Empty;

    /// <summary>Initialise l'outil de recherche/remplacement.</summary>
    /// <param name="entries">Entrées du projet dans lesquelles chercher.</param>
    /// <param name="undoRedo">Pile d'annulation/rétablissement du projet.</param>
    /// <param name="onReplaced">Appelée après un remplacement effectif, pour rafraîchir l'affichage de l'éditeur.</param>
    public FindReplaceViewModel(IReadOnlyList<TranslationEntry> entries, UndoRedoStack undoRedo, Action onReplaced)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(undoRedo);
        ArgumentNullException.ThrowIfNull(onReplaced);

        _entries = entries;
        _undoRedo = undoRedo;
        _onReplaced = onReplaced;

        ReplaceAllCommand = new RelayCommand(ReplaceAll, () => _searchText.Length > 0);
    }

    /// <summary>Texte recherché.</summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value ?? string.Empty))
            {
                ReplaceAllCommand.NotifyCanExecuteChanged();
                RefreshMatchCount();
            }
        }
    }

    /// <summary>Texte de remplacement.</summary>
    public string ReplacementText
    {
        get => _replacementText;
        set => SetProperty(ref _replacementText, value ?? string.Empty);
    }

    /// <summary>Nombre d'entrées dont la traduction contient actuellement le texte recherché.</summary>
    public int MatchCount
    {
        get => _matchCount;
        private set
        {
            if (SetProperty(ref _matchCount, value))
            {
                OnPropertyChanged(nameof(MatchCountText));
            }
        }
    }

    /// <summary>Texte « N entrée(s) correspondante(s) » affiché sous les champs de recherche.</summary>
    public string MatchCountText => string.Format(CultureInfo.CurrentCulture, MatchCountFormat, _matchCount);

    /// <summary>Message affiché après un remplacement (nombre d'entrées modifiées).</summary>
    public string ResultMessage
    {
        get => _resultMessage;
        private set => SetProperty(ref _resultMessage, value);
    }

    /// <summary>Remplace toutes les occurrences du texte recherché par le texte de remplacement.</summary>
    public RelayCommand ReplaceAllCommand { get; }

    private void RefreshMatchCount()
    {
        MatchCount = _searchText.Length == 0 ? 0 : TranslationFindAndReplace.CountMatches(_entries, _searchText);
    }

    private void ReplaceAll()
    {
        CompositeEditCommand? command = TranslationFindAndReplace.PrepareReplaceAll(_entries, _searchText, _replacementText);
        if (command is null)
        {
            ResultMessage = string.Format(CultureInfo.CurrentCulture, NoMatchFormat, _searchText);
            return;
        }

        int replacedCount = command.Count;
        _undoRedo.Execute(command);
        _onReplaced();

        ResultMessage = string.Format(CultureInfo.CurrentCulture, ReplacedCountFormat, replacedCount);
        RefreshMatchCount();
    }
}
