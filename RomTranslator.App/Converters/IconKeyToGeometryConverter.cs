// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RomTranslator.App.Converters;

/// <summary>Convertit une clé d'icône en <see cref="Geometry" /> en cherchant la ressource <c>Icon.clé</c>.</summary>
[ValueConversion(typeof(string), typeof(Geometry))]
public sealed class IconKeyToGeometryConverter : IValueConverter
{
    /// <summary>Préfixe des clés de ressources d'icônes.</summary>
    public const string ResourcePrefix = "Icon.";

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string key || key.Length == 0)
        {
            return DependencyProperty.UnsetValue;
        }

        object? resource = Application.Current?.TryFindResource(ResourcePrefix + key);
        return resource is Geometry geometry ? geometry : DependencyProperty.UnsetValue;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
