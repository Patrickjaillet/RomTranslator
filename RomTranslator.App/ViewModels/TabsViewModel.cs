// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RomTranslator.App.ViewModels;

/// <summary>
/// Ensemble des onglets de la fenêtre principale. Les onglets de consoles sont ajoutés dynamiquement
/// avec <see cref="AddTab" /> ; l'onglet mémorisé à la session précédente est resélectionné dès qu'il apparaît.
/// </summary>
public sealed class TabsViewModel : ObservableObject
{
    private readonly ObservableCollection<TabItemViewModel> _items = new();
    private string? _preferredTabId;
    private TabItemViewModel? _selectedItem;

    /// <summary>Initialise l'ensemble des onglets.</summary>
    /// <param name="preferredTabId">Identifiant de l'onglet à sélectionner dès qu'il est disponible.</param>
    public TabsViewModel(string? preferredTabId = null)
    {
        _preferredTabId = string.IsNullOrWhiteSpace(preferredTabId) ? null : preferredTabId;
        Items = new ReadOnlyObservableCollection<TabItemViewModel>(_items);
    }

    /// <summary>Onglets, dans l'ordre d'affichage.</summary>
    public ReadOnlyObservableCollection<TabItemViewModel> Items { get; }

    /// <summary>Onglet sélectionné. Un choix explicite annule la sélection automatique de l'onglet mémorisé.</summary>
    public TabItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (value is not null && !_items.Contains(value))
            {
                return;
            }

            if (SetProperty(ref _selectedItem, value) && value is not null)
            {
                _preferredTabId = null;
            }
        }
    }

    /// <summary>Ajoute un onglet à la fin de la liste.</summary>
    /// <exception cref="InvalidOperationException">Un onglet porte déjà cet identifiant.</exception>
    public void AddTab(TabItemViewModel tab)
    {
        ArgumentNullException.ThrowIfNull(tab);

        if (Find(tab.Id) is not null)
        {
            throw new InvalidOperationException($"Un onglet d'identifiant « {tab.Id} » existe déjà.");
        }

        _items.Add(tab);

        if (_preferredTabId is not null && string.Equals(_preferredTabId, tab.Id, StringComparison.Ordinal))
        {
            _preferredTabId = null;
            SetProperty(ref _selectedItem, tab, nameof(SelectedItem));
        }
        else if (_selectedItem is null)
        {
            SetProperty(ref _selectedItem, tab, nameof(SelectedItem));
        }
    }

    /// <summary>Retire un onglet non permanent. Si l'onglet retiré était sélectionné, le premier onglet est sélectionné.</summary>
    /// <returns><see langword="true" /> si l'onglet a été retiré.</returns>
    public bool RemoveTab(string id)
    {
        TabItemViewModel? tab = Find(id);
        if (tab is null || tab.IsPermanent)
        {
            return false;
        }

        _items.Remove(tab);

        if (ReferenceEquals(_selectedItem, tab))
        {
            SetProperty(ref _selectedItem, _items.FirstOrDefault(), nameof(SelectedItem));
        }

        return true;
    }

    /// <summary>Sélectionne un onglet à partir de son identifiant.</summary>
    /// <returns><see langword="true" /> si l'onglet existe.</returns>
    public bool SelectTab(string id)
    {
        TabItemViewModel? tab = Find(id);
        if (tab is null)
        {
            return false;
        }

        SelectedItem = tab;
        return true;
    }

    private TabItemViewModel? Find(string id)
    {
        return _items.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.Ordinal));
    }
}
