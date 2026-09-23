// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.Windows.Data;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Localization;

namespace RomTranslator.App.Converters;

/// <summary>Convertit un <see cref="TranslationStatus" /> en libellé traduit.</summary>
[ValueConversion(typeof(TranslationStatus), typeof(string))]
public sealed class TranslationStatusToLabelConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            TranslationStatus.InProgress => Strings.TranslationStatus_InProgress,
            TranslationStatus.Translated => Strings.TranslationStatus_Translated,
            TranslationStatus.Validated => Strings.TranslationStatus_Validated,
            _ => Strings.TranslationStatus_NotTranslated,
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
