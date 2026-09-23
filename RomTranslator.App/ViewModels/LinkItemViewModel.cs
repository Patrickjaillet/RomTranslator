// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using CommunityToolkit.Mvvm.Input;
using RomTranslator.App.Services;

namespace RomTranslator.App.ViewModels;

/// <summary>Lien vers une adresse externe (site, dépôt, contact).</summary>
public sealed class LinkItemViewModel : AppCommandViewModel
{
    /// <summary>Initialise un lien.</summary>
    /// <param name="header">Titre du lien.</param>
    /// <param name="description">Description (aussi utilisée comme infobulle).</param>
    /// <param name="iconKey">Clé de l'icône.</param>
    /// <param name="url">Adresse absolue (http, https ou mailto).</param>
    /// <param name="launcher">Service d'ouverture des adresses.</param>
    /// <param name="reportFailure">Appelée avec l'adresse lorsque l'ouverture échoue.</param>
    public LinkItemViewModel(
        string header,
        string description,
        string iconKey,
        string url,
        IUrlLauncher launcher,
        Action<string> reportFailure)
        : base(header, description, iconKey, CreateCommand(url, launcher, reportFailure))
    {
        Description = description;
        Url = url;
    }

    /// <summary>Description du lien.</summary>
    public string Description { get; }

    /// <summary>Adresse ciblée.</summary>
    public string Url { get; }

    private static RelayCommand CreateCommand(string url, IUrlLauncher launcher, Action<string> reportFailure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentNullException.ThrowIfNull(launcher);
        ArgumentNullException.ThrowIfNull(reportFailure);

        return new RelayCommand(() =>
        {
            if (!launcher.TryOpen(url))
            {
                reportFailure(url);
            }
        });
    }
}
