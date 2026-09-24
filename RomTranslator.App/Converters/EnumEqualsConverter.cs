// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RomTranslator.App.Converters;

/// <summary>
/// Convertit une valeur d'énumération en <see cref="Visibility" /> : <see cref="Visibility.Visible" /> si
/// elle est égale au paramètre de conversion (comparé par son nom textuel), <see cref="Visibility.Collapsed" />
/// sinon. Le paramètre <c>"Invert"</c> ajouté en préfixe (séparé par <c>|</c>, par exemple <c>"Invert|Done"</c>)
/// inverse ce résultat. Utilisé pour afficher une portion d'écran selon l'étape ou l'option choisie dans un énuméré.
/// </summary>
[ValueConversion(typeof(Enum), typeof(Visibility))]
public sealed class EnumEqualsConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string? parameterText = parameter?.ToString();
        bool invert = parameterText is not null && parameterText.StartsWith("Invert|", StringComparison.Ordinal);
        string? comparand = invert ? parameterText!["Invert|".Length..] : parameterText;

        bool matches = value is not null && comparand is not null
            && string.Equals(value.ToString(), comparand, StringComparison.Ordinal);

        return (matches != invert) ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Convertit une valeur d'énumération en <see langword="bool" /> pour un <see cref="System.Windows.Controls.RadioButton" />
/// (coché si la valeur égale le paramètre de conversion, comparé par son nom textuel) ; à l'inverse, cocher
/// le bouton affecte au binding source la valeur d'énumération nommée par le paramètre.
/// </summary>
[ValueConversion(typeof(Enum), typeof(bool))]
public sealed class EnumToRadioButtonConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not null && parameter is not null && string.Equals(value.ToString(), parameter.ToString(), StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not true || parameter is null || targetType is null)
        {
            return Binding.DoNothing;
        }

        return Enum.Parse(Nullable.GetUnderlyingType(targetType) ?? targetType, parameter.ToString()!);
    }
}
