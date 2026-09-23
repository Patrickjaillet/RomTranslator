// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.Core.Configuration;

/// <summary>État persistant de la fenêtre principale.</summary>
public sealed class WindowSettings
{
    /// <summary>Position horizontale (en unités indépendantes de la résolution), ou <see langword="null" /> si inconnue.</summary>
    public double? Left { get; set; }

    /// <summary>Position verticale, ou <see langword="null" /> si inconnue.</summary>
    public double? Top { get; set; }

    /// <summary>Largeur de la fenêtre en état normal, ou <see langword="null" /> si inconnue.</summary>
    public double? Width { get; set; }

    /// <summary>Hauteur de la fenêtre en état normal, ou <see langword="null" /> si inconnue.</summary>
    public double? Height { get; set; }

    /// <summary>Indique si la fenêtre était agrandie à la fermeture.</summary>
    public bool IsMaximized { get; set; }

    /// <summary>Identifiant de l'onglet actif à la fermeture, ou <see langword="null" />.</summary>
    public string? LastActiveTabId { get; set; }
}
