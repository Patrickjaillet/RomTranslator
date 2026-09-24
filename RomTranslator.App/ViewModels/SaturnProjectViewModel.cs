// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomTranslator.Core.Localization;
using RomTranslator.Modules.SegaSaturn.ViewModels;

namespace RomTranslator.App.ViewModels;

/// <summary>
/// Contenu d'onglet d'un projet Saturn : métadonnées du jeu détecté, éditeur de traduction générique, et
/// actions propres au module (édition de la table de caractères, export de la ROM traduite ou d'un patch).
/// </summary>
public sealed class SaturnProjectViewModel : ObservableObject
{
    /// <summary>Initialise le contenu de l'onglet.</summary>
    /// <param name="romInfo">Métadonnées du jeu détecté, ou <see langword="null" /> si elles n'ont pas pu être lues.</param>
    /// <param name="editor">Éditeur de traduction générique, déjà construit pour le projet.</param>
    /// <param name="openCharacterTableEditor">Ouvre l'éditeur de table de caractères du module.</param>
    /// <param name="exportTranslatedRom">
    /// Génère l'image ROM traduite complète (réinjection et écriture disque, potentiellement longues sur une
    /// grande image : exécutée en tâche de fond par <see cref="ExportTranslatedRomCommand" />).
    /// </param>
    /// <param name="exportPatch">Génère un patch IPS (même remarque de performance que <paramref name="exportTranslatedRom" />).</param>
    public SaturnProjectViewModel(
        SaturnRomInfoViewModel? romInfo,
        TranslationEditorViewModel editor,
        Action openCharacterTableEditor,
        Func<Task> exportTranslatedRom,
        Func<Task> exportPatch)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentNullException.ThrowIfNull(openCharacterTableEditor);
        ArgumentNullException.ThrowIfNull(exportTranslatedRom);
        ArgumentNullException.ThrowIfNull(exportPatch);

        RomInfo = romInfo;
        Editor = editor;

        OpenCharacterTableEditorCommand = new RelayCommand(openCharacterTableEditor);
        ExportTranslatedRomCommand = new AsyncRelayCommand(exportTranslatedRom);
        ExportPatchCommand = new AsyncRelayCommand(exportPatch);
    }

    /// <summary>Métadonnées du jeu détecté dans l'image disque, ou <see langword="null" /> si elles n'ont pas pu être lues.</summary>
    public SaturnRomInfoViewModel? RomInfo { get; }

    /// <summary>Texte d'en-tête affichant le titre du jeu, ou un texte de substitution si absent.</summary>
    public string HeaderText => RomInfo is null ? Strings.Saturn_Project_UnknownGame : RomInfo.Title;

    /// <summary>Éditeur de traduction générique du projet.</summary>
    public TranslationEditorViewModel Editor { get; }

    /// <summary>Ouvre l'éditeur de la table de caractères du projet.</summary>
    public RelayCommand OpenCharacterTableEditorCommand { get; }

    /// <summary>Génère l'image ROM traduite complète.</summary>
    public AsyncRelayCommand ExportTranslatedRomCommand { get; }

    /// <summary>Génère un patch IPS à partir de la ROM source et de la ROM traduite.</summary>
    public AsyncRelayCommand ExportPatchCommand { get; }
}
