// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RomTranslator.Core.Information;
using RomTranslator.Core.Localization;

namespace RomTranslator.App.ViewModels;

/// <summary>Écran d'accueil : présentation de l'application et liens utiles.</summary>
public sealed class HomeViewModel
{
    private static readonly CompositeFormat WelcomeFormat = CompositeFormat.Parse(Strings.Home_Welcome);
    private static readonly CompositeFormat VersionPrefixFormat = CompositeFormat.Parse(Strings.Home_VersionPrefix);

    /// <summary>Initialise l'écran d'accueil.</summary>
    public HomeViewModel(ApplicationInfo info, IReadOnlyList<LinkItemViewModel> links)
    {
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(links);

        WelcomeText = string.Format(CultureInfo.CurrentCulture, WelcomeFormat, info.Name);
        VersionText = string.Format(CultureInfo.CurrentCulture, VersionPrefixFormat, info.Version);
        CopyrightText = info.Copyright;
        Links = links;
    }

    /// <summary>Message de bienvenue avec le nom du produit.</summary>
    public string WelcomeText { get; }

    /// <summary>Version affichée.</summary>
    public string VersionText { get; }

    /// <summary>Mention de copyright.</summary>
    public string CopyrightText { get; }

    /// <summary>Mention de licence.</summary>
    public string LicenseText => Strings.Home_License;

    /// <summary>Liens vers le site, le code source, les versions et le contact.</summary>
    public IReadOnlyList<LinkItemViewModel> Links { get; }
}
