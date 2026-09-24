// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.App.ViewModels;

/// <summary>Une entrée du sélecteur de langue des paramètres.</summary>
/// <param name="LanguageCode">Code de langue (« fr », « en »), ou <see langword="null" /> pour suivre le système.</param>
/// <param name="DisplayName">Libellé affiché (déjà traduit).</param>
public sealed record LanguageOptionViewModel(string? LanguageCode, string DisplayName)
{
    /// <inheritdoc />
    public override string ToString() => DisplayName;
}
