// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RomTranslator.Modules.SegaSaturn.CharacterTables;

/// <summary>
/// Éditeur de table de caractères : associe des séquences d'octets à des caractères affichés, avec
/// validation de cohérence en direct (pas de séquence d'octets ni de texte associé à deux entrées
/// différentes) et chargement/enregistrement au format <c>.tbl</c>.
/// </summary>
public sealed class CharacterTableEditorViewModel : ObservableObject
{
    private string? _loadedPath;
    private CharacterTableEntryViewModel? _selectedEntry;

    /// <summary>Initialise l'éditeur, vide ou à partir d'une table déjà chargée.</summary>
    /// <param name="file">Table à éditer, ou <see langword="null" /> pour démarrer une table vide.</param>
    public CharacterTableEditorViewModel(CharacterTableFile? file = null)
    {
        Entries = new ObservableCollection<CharacterTableEntryViewModel>(
            (file?.Entries ?? Array.Empty<CharacterTableEntry>()).Select(entry => new CharacterTableEntryViewModel(entry)));

        foreach (CharacterTableEntryViewModel entry in Entries)
        {
            entry.PropertyChanged += OnEntryPropertyChanged;
        }

        AddEntryCommand = new RelayCommand(AddEntry);
        RemoveEntryCommand = new RelayCommand(RemoveSelectedEntry, () => SelectedEntry is not null);

        RefreshValidationIssues();
    }

    /// <summary>Entrées de la table, dans l'ordre d'édition.</summary>
    public ObservableCollection<CharacterTableEntryViewModel> Entries { get; }

    /// <summary>Entrée actuellement sélectionnée, ou <see langword="null" />.</summary>
    public CharacterTableEntryViewModel? SelectedEntry
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

    /// <summary>Problèmes de cohérence détectés dans la table (doublons de séquence ou de texte), recalculés à chaque modification.</summary>
    public IReadOnlyList<string> ValidationIssues { get; private set; } = Array.Empty<string>();

    /// <summary>Indique si la table ne présente aucun problème de cohérence.</summary>
    public bool IsValid => ValidationIssues.Count == 0;

    /// <summary>Ajoute une entrée vide à la table.</summary>
    public RelayCommand AddEntryCommand { get; }

    /// <summary>Retire l'entrée sélectionnée de la table.</summary>
    public RelayCommand RemoveEntryCommand { get; }

    /// <summary>Charge une table depuis un fichier <c>.tbl</c> et remplace le contenu de l'éditeur.</summary>
    /// <param name="path">Chemin du fichier <c>.tbl</c> à charger.</param>
    public void Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        CharacterTableFile file = CharacterTableFileReader.Read(path);

        foreach (CharacterTableEntryViewModel entry in Entries)
        {
            entry.PropertyChanged -= OnEntryPropertyChanged;
        }

        Entries.Clear();
        foreach (CharacterTableEntry entry in file.Entries)
        {
            CharacterTableEntryViewModel viewModel = new(entry);
            viewModel.PropertyChanged += OnEntryPropertyChanged;
            Entries.Add(viewModel);
        }

        _loadedPath = path;
        RefreshValidationIssues();
    }

    /// <summary>Enregistre la table dans un fichier <c>.tbl</c>.</summary>
    /// <param name="path">Chemin du fichier <c>.tbl</c> à créer.</param>
    /// <exception cref="FormatException">Une entrée a une séquence d'octets invalide.</exception>
    public void Save(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        List<CharacterTableEntry> entries = Entries.Select(entry => entry.ToEntry()).ToList();
        CharacterTableFileWriter.Write(new CharacterTableFile(entries, NewLineBytes: null, EndOfTextBytes: null), path);
        _loadedPath = path;
    }

    private void AddEntry()
    {
        CharacterTableEntryViewModel entry = new();
        entry.PropertyChanged += OnEntryPropertyChanged;
        Entries.Add(entry);
        SelectedEntry = entry;
    }

    private void RemoveSelectedEntry()
    {
        if (SelectedEntry is null)
        {
            return;
        }

        SelectedEntry.PropertyChanged -= OnEntryPropertyChanged;
        Entries.Remove(SelectedEntry);
        SelectedEntry = null;
        RefreshValidationIssues();
    }

    private void OnEntryPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        RefreshValidationIssues();
    }

    private void RefreshValidationIssues()
    {
        List<CharacterTableEntry> validEntries = new();
        foreach (CharacterTableEntryViewModel entry in Entries)
        {
            if (entry.IsValid)
            {
                validEntries.Add(entry.ToEntry());
            }
        }

        // Le nom n'est pas affiché ici : Validate() ne l'utilise pas, seule la cohérence des entrées compte.
        SaturnCharacterTable table = new(_loadedPath ?? "-", new CharacterTableFile(validEntries, null, null));

        ValidationIssues = table.Validate();
        OnPropertyChanged(nameof(ValidationIssues));
        OnPropertyChanged(nameof(IsValid));
    }
}
