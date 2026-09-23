// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RomTranslator.App.ViewModels;

/// <summary>Barre de statut : message d'information, progression et langue active.</summary>
public sealed class StatusBarViewModel : ObservableObject
{
    private string _message = string.Empty;
    private double _progress;
    private bool _isProgressVisible;
    private bool _isIndeterminate;
    private string _languageCode;

    /// <summary>Initialise la barre de statut.</summary>
    /// <param name="languageCode">Code court de la langue active (par exemple « FR »).</param>
    public StatusBarViewModel(string languageCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        _languageCode = languageCode;
    }

    /// <summary>Message d'information courant.</summary>
    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    /// <summary>Progression de 0 à 100.</summary>
    public double Progress
    {
        get => _progress;
        private set => SetProperty(ref _progress, value);
    }

    /// <summary>Indique si la barre de progression est affichée.</summary>
    public bool IsProgressVisible
    {
        get => _isProgressVisible;
        private set => SetProperty(ref _isProgressVisible, value);
    }

    /// <summary>Indique que la progression est de durée indéterminée.</summary>
    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        private set => SetProperty(ref _isIndeterminate, value);
    }

    /// <summary>Code court de la langue active.</summary>
    public string LanguageCode
    {
        get => _languageCode;
        set => SetProperty(ref _languageCode, value);
    }

    /// <summary>Affiche un message d'information.</summary>
    public void ReportMessage(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        Message = message;
    }

    /// <summary>Affiche la barre de progression, à zéro.</summary>
    public void BeginProgress(bool isIndeterminate = false)
    {
        IsIndeterminate = isIndeterminate;
        Progress = 0;
        IsProgressVisible = true;
    }

    /// <summary>Met à jour la progression (valeur ramenée entre 0 et 100).</summary>
    public void ReportProgress(double percent)
    {
        Progress = double.IsNaN(percent) ? 0 : Math.Clamp(percent, 0, 100);
    }

    /// <summary>Masque la barre de progression.</summary>
    public void EndProgress()
    {
        IsProgressVisible = false;
        IsIndeterminate = false;
        Progress = 0;
    }
}
