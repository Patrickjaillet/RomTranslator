// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RomTranslator.App.Controls;

/// <summary>
/// Icône vectorielle : dessine une <see cref="Geometry" /> issue d'un fichier SVG (grille 24 x 24) avec la couleur
/// de premier plan héritée, ce qui suit automatiquement les états des boutons et des menus.
/// </summary>
public partial class SvgIcon : UserControl
{
    /// <summary>Géométrie à dessiner.</summary>
    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
        nameof(Data), typeof(Geometry), typeof(SvgIcon), new PropertyMetadata(null));

    /// <summary>Taille de l'icône en unités indépendantes de la résolution.</summary>
    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
        nameof(IconSize), typeof(double), typeof(SvgIcon), new PropertyMetadata(16d));

    /// <summary>Épaisseur du tracé sur la grille 24 x 24.</summary>
    public static readonly DependencyProperty StrokeWidthProperty = DependencyProperty.Register(
        nameof(StrokeWidth), typeof(double), typeof(SvgIcon), new PropertyMetadata(2d));

    public SvgIcon()
    {
        InitializeComponent();
    }

    /// <summary>Géométrie à dessiner.</summary>
    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// <summary>Taille de l'icône.</summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>Épaisseur du tracé.</summary>
    public double StrokeWidth
    {
        get => (double)GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }
}
