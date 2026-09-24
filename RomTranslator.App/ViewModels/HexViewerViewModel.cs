// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomTranslator.Core.Binary;
using RomTranslator.Core.Localization;

namespace RomTranslator.App.ViewModels;

/// <summary>
/// Visualiseur hexadécimal générique en lecture seule, pour l'inspection d'une image ROM. Affiche une page
/// de lignes à la fois plutôt que le fichier entier, pour rester utilisable sur des images volumineuses.
/// </summary>
public sealed class HexViewerViewModel : ObservableObject
{
    /// <summary>Nombre de lignes affichées par page.</summary>
    public const int PageSize = 512;

    private static readonly CompositeFormat PageIndicatorFormat = CompositeFormat.Parse(Strings.HexViewer_PageIndicator);
    private static readonly CompositeFormat FileSizeLabelFormat = CompositeFormat.Parse(Strings.HexViewer_FileSizeLabel);

    private readonly HexDumpReader _reader;
    private long _currentPage;

    /// <summary>Initialise le visualiseur pour un fichier.</summary>
    /// <param name="filePath">Chemin du fichier à inspecter.</param>
    public HexViewerViewModel(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        FilePath = filePath;
        _reader = new HexDumpReader(filePath);
        Lines = new ObservableCollection<HexDumpLine>();

        PreviousPageCommand = new RelayCommand(() => CurrentPage--, () => CurrentPage > 0);
        NextPageCommand = new RelayCommand(() => CurrentPage++, () => CurrentPage < PageCount - 1);

        LoadCurrentPage();
    }

    /// <summary>Chemin du fichier inspecté.</summary>
    public string FilePath { get; }

    /// <summary>Taille totale du fichier, en octets.</summary>
    public long FileLength => _reader.Length;

    /// <summary>Lignes de la page actuellement affichée.</summary>
    public ObservableCollection<HexDumpLine> Lines { get; }

    /// <summary>Nombre total de pages.</summary>
    public long PageCount => Math.Max(1, (_reader.LineCount + PageSize - 1) / PageSize);

    /// <summary>Index de la page actuellement affichée (0-based).</summary>
    public long CurrentPage
    {
        get => _currentPage;
        set
        {
            long clamped = Math.Clamp(value, 0, PageCount - 1);
            if (SetProperty(ref _currentPage, clamped))
            {
                LoadCurrentPage();
                PreviousPageCommand.NotifyCanExecuteChanged();
                NextPageCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(PageIndicatorText));
            }
        }
    }

    /// <summary>Texte « Page N sur M » affiché au-dessus de la liste des lignes.</summary>
    public string PageIndicatorText =>
        string.Format(CultureInfo.CurrentCulture, PageIndicatorFormat, CurrentPage + 1, PageCount);

    /// <summary>Texte « Taille du fichier : N octets ».</summary>
    public string FileSizeText => string.Format(CultureInfo.CurrentCulture, FileSizeLabelFormat, FileLength);

    /// <summary>Affiche la page précédente.</summary>
    public RelayCommand PreviousPageCommand { get; }

    /// <summary>Affiche la page suivante.</summary>
    public RelayCommand NextPageCommand { get; }

    private void LoadCurrentPage()
    {
        Lines.Clear();
        foreach (HexDumpLine line in _reader.ReadLines(_currentPage * PageSize, PageSize))
        {
            Lines.Add(line);
        }
    }
}
