// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.App.ViewModels;

/// <summary>
/// Clés des icônes de l'application. Chaque clé correspond à un fichier <c>Assets/Icons/clé.svg</c> et à une
/// ressource <c>Icon.clé</c> de <c>Resources/Icons.xaml</c> (cohérence vérifiée par les tests automatiques).
/// </summary>
public static class IconKeys
{
    /// <summary>Accueil.</summary>
    public const string Home = "home";

    /// <summary>Nouveau projet.</summary>
    public const string NewProject = "file-plus";

    /// <summary>Ouvrir un projet.</summary>
    public const string Open = "folder";

    /// <summary>Enregistrer.</summary>
    public const string Save = "save";

    /// <summary>Exporter.</summary>
    public const string Export = "export";

    /// <summary>Paramètres.</summary>
    public const string Settings = "settings";

    /// <summary>Aide.</summary>
    public const string Help = "help";

    /// <summary>À propos.</summary>
    public const string About = "info";

    /// <summary>Annuler.</summary>
    public const string Undo = "undo";

    /// <summary>Rétablir.</summary>
    public const string Redo = "redo";

    /// <summary>Console de jeux (icône d'onglet par défaut).</summary>
    public const string Console = "gamepad";

    /// <summary>Site web.</summary>
    public const string Website = "globe";

    /// <summary>Code source.</summary>
    public const string Repository = "code";

    /// <summary>Téléchargement des versions.</summary>
    public const string Releases = "download";

    /// <summary>Contact.</summary>
    public const string Contact = "mail";

    /// <summary>Quitter.</summary>
    public const string Exit = "exit";

    /// <summary>Recherche.</summary>
    public const string Search = "search";

    /// <summary>Filtre.</summary>
    public const string Filter = "filter";

    /// <summary>Validation, entrée traitée.</summary>
    public const string Check = "check";

    /// <summary>Alerte (dépassement de la limite de longueur).</summary>
    public const string AlertTriangle = "alert-triangle";
}
