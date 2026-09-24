// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using RomTranslator.Core.Projects;

namespace RomTranslator.App.ViewModels;

/// <summary>Une entrée éditable du glossaire du projet.</summary>
public sealed class GlossaryEntryViewModel : ObservableObject
{
    private readonly GlossaryEntry _entry;

    /// <summary>Enveloppe une entrée de glossaire existante.</summary>
    public GlossaryEntryViewModel(GlossaryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _entry = entry;
    }

    /// <summary>Entrée de glossaire enveloppée.</summary>
    public GlossaryEntry Entry => _entry;

    /// <summary>Terme source.</summary>
    public string SourceTerm => _entry.SourceTerm;

    /// <summary>Traduction retenue pour ce terme.</summary>
    public string TargetTerm
    {
        get => _entry.TargetTerm;
        set
        {
            if (!string.Equals(_entry.TargetTerm, value, StringComparison.Ordinal))
            {
                _entry.TargetTerm = value ?? string.Empty;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Remarque libre associée au terme.</summary>
    public string? Note
    {
        get => _entry.Note;
        set
        {
            if (!string.Equals(_entry.Note, value, StringComparison.Ordinal))
            {
                _entry.Note = value;
                OnPropertyChanged();
            }
        }
    }
}
