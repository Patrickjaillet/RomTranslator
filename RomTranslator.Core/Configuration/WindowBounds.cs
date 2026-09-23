// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.Core.Configuration;

/// <summary>Rectangle en unités indépendantes de la résolution (indépendant de toute bibliothèque d'interface).</summary>
/// <param name="Left">Bord gauche.</param>
/// <param name="Top">Bord supérieur.</param>
/// <param name="Width">Largeur.</param>
/// <param name="Height">Hauteur.</param>
public readonly record struct WindowBounds(double Left, double Top, double Width, double Height)
{
    /// <summary>Bord droit.</summary>
    public double Right => Left + Width;

    /// <summary>Bord inférieur.</summary>
    public double Bottom => Top + Height;
}
