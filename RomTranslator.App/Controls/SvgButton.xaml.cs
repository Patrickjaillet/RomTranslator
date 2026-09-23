// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RomTranslator.App.Controls;

/// <summary>Bouton Fluent dont l'icône est une géométrie SVG, avec un libellé facultatif.</summary>
public partial class SvgButton : UserControl
{
    /// <summary>Géométrie de l'icône.</summary>
    public static readonly DependencyProperty IconDataProperty = DependencyProperty.Register(
        nameof(IconData), typeof(Geometry), typeof(SvgButton), new PropertyMetadata(null));

    /// <summary>Libellé affiché à droite de l'icône.</summary>
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label), typeof(string), typeof(SvgButton), new PropertyMetadata(null, OnLabelChanged));

    /// <summary>Taille de l'icône.</summary>
    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
        nameof(IconSize), typeof(double), typeof(SvgButton), new PropertyMetadata(16d));

    /// <summary>Commande exécutée au clic.</summary>
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(SvgButton), new PropertyMetadata(null));

    /// <summary>Paramètre transmis à la commande.</summary>
    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        nameof(CommandParameter), typeof(object), typeof(SvgButton), new PropertyMetadata(null));

    private static readonly DependencyPropertyKey _hasLabelPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(HasLabel), typeof(bool), typeof(SvgButton), new PropertyMetadata(false));

    /// <summary>Indique qu'un libellé est défini.</summary>
    public static readonly DependencyProperty HasLabelProperty = _hasLabelPropertyKey.DependencyProperty;

    public SvgButton()
    {
        InitializeComponent();
    }

    /// <summary>Géométrie de l'icône.</summary>
    public Geometry? IconData
    {
        get => (Geometry?)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    /// <summary>Libellé affiché à droite de l'icône.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Taille de l'icône.</summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>Commande exécutée au clic.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Paramètre transmis à la commande.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>Indique qu'un libellé est défini.</summary>
    public bool HasLabel => (bool)GetValue(HasLabelProperty);

    private static void OnLabelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        sender.SetValue(_hasLabelPropertyKey, !string.IsNullOrEmpty(e.NewValue as string));
    }
}
