// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.Core.Abstractions;

namespace RomTranslator.Core.Projects;

/// <summary>
/// Contenu chargé d'un projet de traduction, prêt à être présenté par l'application : le projet lui-même,
/// sa table de caractères résolue et la contrainte de longueur du module (si le module en définit une).
/// Ne dépend d'aucun type propre à l'interface graphique : c'est à l'application de construire l'éditeur de
/// traduction (ou toute autre présentation) à partir de ce contenu, puisqu'un module console n'a pas le
/// droit de référencer <c>RomTranslator.App</c> (règle de dépendance à sens unique).
/// </summary>
/// <param name="Project">Projet de traduction chargé.</param>
/// <param name="CharacterTable">Table de caractères résolue pour ce projet.</param>
/// <param name="LengthPolicy">Contrainte de longueur du module, ou <see langword="null" /> si aucune n'est définie.</param>
public sealed record ConsoleProjectContext(TranslationProject Project, ICharacterTable CharacterTable, ITranslationLengthPolicy? LengthPolicy);

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

    /// <summary>Charge un projet de traduction et résout son contenu, prêt à être présenté par l'application.</summary>
    /// <param name="projectPath">Chemin du fichier de projet de traduction à ouvrir.</param>
    ConsoleProjectContext LoadProjectContext(string projectPath);
}
