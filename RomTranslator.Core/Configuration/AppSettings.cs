// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.Core.Configuration;

/// <summary>Paramètres de l'application, enregistrés dans <c>config/settings.json</c>.</summary>
public sealed class AppSettings
{
    /// <summary>Version courante du format du fichier de paramètres.</summary>
    public const int CurrentSchemaVersion = 1;

    private WindowSettings _window = new();

    /// <summary>Version du format du fichier, pour les migrations futures.</summary>
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    /// <summary>État de la fenêtre principale (jamais <see langword="null" />).</summary>
    public WindowSettings Window
    {
        get => _window;
        set => _window = value ?? new WindowSettings();
    }

    /// <summary>
    /// Langue d'interface choisie explicitement (« fr » ou « en »), ou <see langword="null" /> pour suivre
    /// la langue du système. Le changement de langue nécessite un redémarrage de l'application.
    /// </summary>
    public string? LanguageCode { get; set; }
}
