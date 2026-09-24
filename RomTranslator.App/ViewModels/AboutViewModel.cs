// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.Text;
using RomTranslator.Core.Information;
using RomTranslator.Core.Localization;

namespace RomTranslator.App.ViewModels;

/// <summary>Modèle de vue de la boîte de dialogue « À propos ».</summary>
public sealed class AboutViewModel
{
    private static readonly CompositeFormat VersionPrefixFormat = CompositeFormat.Parse(Strings.Home_VersionPrefix);

    /// <summary>Initialise la boîte « À propos ».</summary>
    /// <param name="info">Identité de l'application.</param>
    /// <param name="website">Lien vers le site officiel.</param>
    /// <param name="repository">Lien vers le code source.</param>
    public AboutViewModel(ApplicationInfo info, LinkItemViewModel website, LinkItemViewModel repository)
    {
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(website);
        ArgumentNullException.ThrowIfNull(repository);

        ProductName = info.Name;
        VersionText = string.Format(CultureInfo.CurrentCulture, VersionPrefixFormat, info.Version);
        CopyrightText = info.Copyright;
        Website = website;
        Repository = repository;
    }

    /// <summary>Nom du produit.</summary>
    public string ProductName { get; }

    /// <summary>Version affichée.</summary>
    public string VersionText { get; }

    /// <summary>Mention de copyright.</summary>
    public string CopyrightText { get; }

    /// <summary>Mention de licence.</summary>
    public string LicenseText => Strings.Home_License;

    /// <summary>Lien vers le site officiel.</summary>
    public LinkItemViewModel Website { get; }

    /// <summary>Lien vers le code source.</summary>
    public LinkItemViewModel Repository { get; }
}
