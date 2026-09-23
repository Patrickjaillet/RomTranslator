// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RomTranslator.App.Converters;

/// <summary>
/// Convertit une référence ou une chaîne en <see cref="Visibility" /> : <see cref="Visibility.Visible" />
/// si non nulle (et non vide pour une chaîne), <see cref="Visibility.Collapsed" /> sinon. Le paramètre
/// <c>"Invert"</c> inverse ce résultat.
/// </summary>
[ValueConversion(typeof(object), typeof(Visibility))]
public sealed class NullToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool hasValue = value is string text ? text.Length > 0 : value is not null;
        bool invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);

        return hasValue != invert ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
