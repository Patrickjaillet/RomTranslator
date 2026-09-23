// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace RomTranslator.App.Services;

/// <summary>
/// Impose le thème clair pendant toute la durée de vie de l'application : applique le thème clair et un
/// accent fixe au démarrage, puis annule toute bascule vers le thème sombre. Le suivi automatique du thème
/// de Windows n'est jamais activé. Les thèmes à contraste élevé de Windows restent respectés (accessibilité).
/// </summary>
internal sealed class LightThemeEnforcer
{
    private readonly Color _accent;
    private bool _isApplying;

    public LightThemeEnforcer(Color accent)
    {
        _accent = accent;
    }

    /// <summary>Applique le thème clair puis surveille les changements de thème.</summary>
    public void Start()
    {
        Apply();

        ApplicationThemeManager.Changed += (theme, _) =>
        {
            if (!_isApplying && theme == ApplicationTheme.Dark)
            {
                Apply();
            }
        };
    }

    private void Apply()
    {
        _isApplying = true;

        try
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Light, Wpf.Ui.Controls.WindowBackdropType.Mica, updateAccent: false);
            ApplicationAccentColorManager.Apply(_accent, ApplicationTheme.Light);
        }
        finally
        {
            _isApplying = false;
        }
    }
}
