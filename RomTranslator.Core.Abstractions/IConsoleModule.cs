// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.Core.Abstractions;

/// <summary>
/// Point d'entrée d'un module de traduction propre à une console (par exemple Sega Saturn). Un module
/// assemble les services concrets (chargement de ROM, extraction, injection, tables de caractères) et
/// fournit son identité à la fenêtre principale, qui l'affiche dans un onglet dédié.
/// </summary>
public interface IConsoleModule
{
    /// <summary>
    /// Identifiant technique stable du module (minuscules, sans espace, par exemple <c>"sega-saturn"</c>).
    /// Sert de clé pour mémoriser l'onglet actif d'une session à l'autre ; ne doit jamais changer une fois publié.
    /// </summary>
    string Id { get; }

    /// <summary>Nom de la console affiché dans l'interface (déjà traduit dans la langue active).</summary>
    string DisplayName { get; }

    /// <summary>Description courte du module (déjà traduite), affichée dans les écrans de sélection de console.</summary>
    string Description { get; }

    /// <summary>Clé de l'icône du module (voir la bibliothèque d'icônes SVG de l'application).</summary>
    string IconKey { get; }

    /// <summary>Version SemVer du module, indépendante de la version de l'application.</summary>
    string Version { get; }

    /// <summary>Chargeur d'images ROM propre à cette console.</summary>
    IRomLoader RomLoader { get; }

    /// <summary>Validateur d'images ROM propre à cette console.</summary>
    IRomValidator RomValidator { get; }

    /// <summary>Extracteur de texte propre à cette console.</summary>
    ITextExtractor TextExtractor { get; }

    /// <summary>Injecteur de texte propre à cette console.</summary>
    ITextInjector TextInjector { get; }

    /// <summary>
    /// Crée le modèle de vue racine de l'onglet du module pour un projet de traduction donné.
    /// Le type exact du modèle de vue est propre au module ; l'application y associe une vue par un
    /// modèle de données XAML, comme pour l'écran d'accueil.
    /// </summary>
    /// <param name="projectPath">Chemin du fichier de projet de traduction ouvert.</param>
    object CreateTabContent(string projectPath);
}
