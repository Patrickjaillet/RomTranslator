// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using RomTranslator.Core.Abstractions;

namespace RomTranslator.App.Converters;

/// <summary>Convertit un <see cref="TranslationStatus" /> en couleur d'accent pour son badge d'affichage.</summary>
[ValueConversion(typeof(TranslationStatus), typeof(Brush))]
public sealed class TranslationStatusToBrushConverter : IValueConverter
{
    /// <summary>Couleur du statut « non traduit ».</summary>
    public static readonly SolidColorBrush NotTranslatedBrush = new(Color.FromRgb(0x8A, 0x8A, 0x8E));

    /// <summary>Couleur du statut « en cours ».</summary>
    public static readonly SolidColorBrush InProgressBrush = new(Color.FromRgb(0xD1, 0x8A, 0x1D));

    /// <summary>Couleur du statut « traduit ».</summary>
    public static readonly SolidColorBrush TranslatedBrush = new(Color.FromRgb(0x1D, 0x5F, 0xD1));

    /// <summary>Couleur du statut « validé ».</summary>
    public static readonly SolidColorBrush ValidatedBrush = new(Color.FromRgb(0x1D, 0x8A, 0x4A));

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            TranslationStatus.InProgress => InProgressBrush,
            TranslationStatus.Translated => TranslatedBrush,
            TranslationStatus.Validated => ValidatedBrush,
            _ => NotTranslatedBrush,
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
