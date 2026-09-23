// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using RomTranslator.Core.Information;

namespace RomTranslator.App.ViewModels;

/// <summary>Écran d'accueil : présentation de l'application et liens utiles.</summary>
public sealed class HomeViewModel
{
    /// <summary>Initialise l'écran d'accueil.</summary>
    public HomeViewModel(ApplicationInfo info, IReadOnlyList<LinkItemViewModel> links)
    {
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(links);

        ProductName = info.Name;
        VersionText = "Version " + info.Version;
        CopyrightText = info.Copyright;
        Links = links;
    }

    /// <summary>Nom du produit.</summary>
    public string ProductName { get; }

    /// <summary>Version affichée.</summary>
    public string VersionText { get; }

    /// <summary>Mention de copyright.</summary>
    public string CopyrightText { get; }

    /// <summary>Mention de licence.</summary>
    public string LicenseText => "Logiciel libre distribué sous licence GNU GPL v3.";

    /// <summary>Liens vers le site, le code source, les versions et le contact.</summary>
    public IReadOnlyList<LinkItemViewModel> Links { get; }
}
