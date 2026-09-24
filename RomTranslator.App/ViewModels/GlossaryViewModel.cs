// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomTranslator.Core.Projects;

namespace RomTranslator.App.ViewModels;

/// <summary>
/// Glossaire du projet : termes récurrents et leur traduction retenue, pour la cohérence terminologique.
/// Les incohérences entre le glossaire et les traductions déjà saisies sont recalculées à chaque changement.
/// </summary>
public sealed class GlossaryViewModel : ObservableObject
{
    private readonly TranslationProject _project;
    private GlossaryEntryViewModel? _selectedEntry;

    /// <summary>Initialise le glossaire pour un projet chargé.</summary>
    public GlossaryViewModel(TranslationProject project)
    {
        ArgumentNullException.ThrowIfNull(project);

        _project = project;
        Entries = new ObservableCollection<GlossaryEntryViewModel>(project.Glossary.Select(entry => new GlossaryEntryViewModel(entry)));

        AddEntryCommand = new RelayCommand<string>(AddEntry, sourceTerm => !string.IsNullOrWhiteSpace(sourceTerm));
        RemoveEntryCommand = new RelayCommand(RemoveSelectedEntry, () => SelectedEntry is not null);

        RefreshInconsistencies();
    }

    /// <summary>Entrées du glossaire.</summary>
    public ObservableCollection<GlossaryEntryViewModel> Entries { get; }

    /// <summary>Entrée actuellement sélectionnée, ou <see langword="null" />.</summary>
    public GlossaryEntryViewModel? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            if (SetProperty(ref _selectedEntry, value))
            {
                RemoveEntryCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Entrées déjà traduites dont la traduction ne reprend pas le terme retenu par le glossaire pour un
    /// terme source qu'elles contiennent.
    /// </summary>
    public IReadOnlyList<GlossaryInconsistency> Inconsistencies { get; private set; } = Array.Empty<GlossaryInconsistency>();

    /// <summary>Indique si des incohérences terminologiques ont été détectées.</summary>
    public bool HasInconsistencies => Inconsistencies.Count > 0;

    /// <summary>Ajoute une entrée pour le terme source indiqué.</summary>
    public RelayCommand<string> AddEntryCommand { get; }

    /// <summary>Retire l'entrée sélectionnée du glossaire.</summary>
    public RelayCommand RemoveEntryCommand { get; }

    /// <summary>Recalcule les incohérences entre le glossaire et les traductions du projet.</summary>
    public void RefreshInconsistencies()
    {
        Inconsistencies = GlossaryConsistencyChecker.FindInconsistencies(_project.Entries, _project.Glossary);
        OnPropertyChanged(nameof(Inconsistencies));
        OnPropertyChanged(nameof(HasInconsistencies));
    }

    private void AddEntry(string? sourceTerm)
    {
        if (string.IsNullOrWhiteSpace(sourceTerm))
        {
            return;
        }

        GlossaryEntry entry = new(sourceTerm, targetTerm: string.Empty);
        _project.Glossary.Add(entry);

        GlossaryEntryViewModel viewModel = new(entry);
        Entries.Add(viewModel);
        SelectedEntry = viewModel;
    }

    private void RemoveSelectedEntry()
    {
        if (SelectedEntry is null)
        {
            return;
        }

        _project.Glossary.Remove(SelectedEntry.Entry);
        Entries.Remove(SelectedEntry);
        SelectedEntry = null;
        RefreshInconsistencies();
    }
}
