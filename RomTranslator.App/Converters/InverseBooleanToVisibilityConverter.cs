// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RomTranslator.App.Converters;

/// <summary>
/// Convertit un booléen en <see cref="Visibility" /> inversée : <see langword="false" /> devient
/// <see cref="Visibility.Visible" />, <see langword="true" /> devient <see cref="Visibility.Collapsed" />.
/// </summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class InverseBooleanToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool boolean = value is bool b && b;
        return boolean ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
