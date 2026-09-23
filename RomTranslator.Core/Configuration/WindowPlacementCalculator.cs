// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;

namespace RomTranslator.Core.Configuration;

/// <summary>
/// Calcule la position et la taille à appliquer à la fenêtre à partir des valeurs enregistrées, en les
/// ramenant dans l'écran virtuel courant (par exemple après le débranchement d'un second écran).
/// </summary>
public static class WindowPlacementCalculator
{
    /// <summary>Calcule les dimensions à appliquer.</summary>
    /// <param name="settings">Valeurs enregistrées.</param>
    /// <param name="virtualScreen">Zone couverte par l'ensemble des écrans.</param>
    /// <param name="minWidth">Largeur minimale de la fenêtre.</param>
    /// <param name="minHeight">Hauteur minimale de la fenêtre.</param>
    /// <returns>Les dimensions à appliquer, ou <see langword="null" /> si aucune valeur exploitable n'est enregistrée.</returns>
    public static WindowBounds? Resolve(WindowSettings settings, WindowBounds virtualScreen, double minWidth, double minHeight)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.Left is not { } left
            || settings.Top is not { } top
            || settings.Width is not { } width
            || settings.Height is not { } height)
        {
            return null;
        }

        if (!double.IsFinite(left) || !double.IsFinite(top) || !double.IsFinite(width) || !double.IsFinite(height))
        {
            return null;
        }

        if (virtualScreen.Width <= 0 || virtualScreen.Height <= 0)
        {
            return null;
        }

        double clampedWidth = Math.Clamp(width, Math.Min(minWidth, virtualScreen.Width), virtualScreen.Width);
        double clampedHeight = Math.Clamp(height, Math.Min(minHeight, virtualScreen.Height), virtualScreen.Height);
        double clampedLeft = Math.Clamp(left, virtualScreen.Left, virtualScreen.Right - clampedWidth);
        double clampedTop = Math.Clamp(top, virtualScreen.Top, virtualScreen.Bottom - clampedHeight);

        return new WindowBounds(clampedLeft, clampedTop, clampedWidth, clampedHeight);
    }
}
