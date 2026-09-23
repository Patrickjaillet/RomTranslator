// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RomTranslator.App.Converters;

/// <summary>
/// Convertit un entier en <see cref="Visibility" /> : <see cref="Visibility.Visible" /> si strictement
/// positif, <see cref="Visibility.Collapsed" /> sinon. Le paramètre <c>"Invert"</c> inverse ce résultat.
/// </summary>
[ValueConversion(typeof(int), typeof(Visibility))]
public sealed class CountToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool hasItems = value is int count && count > 0;
        bool invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);

        return hasItems != invert ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
