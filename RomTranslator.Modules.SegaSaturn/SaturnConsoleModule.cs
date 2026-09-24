// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Localization;
using RomTranslator.Core.Projects;
using RomTranslator.Modules.SegaSaturn.CharacterTables;
using RomTranslator.Modules.SegaSaturn.Translation;

namespace RomTranslator.Modules.SegaSaturn;

/// <summary>
/// Module console Sega Saturn : assemble les services du module (chargement, validation, extraction,
/// réinjection) derrière le contrat <see cref="IConsoleModule" />.
/// </summary>
public sealed class SaturnConsoleModule : IConsoleModule
{
    /// <summary>Identifiant stable du module, utilisé comme <see cref="TranslationProject.ConsoleId" />.</summary>
    public const string ModuleId = "sega-saturn";

    private readonly string _applicationDirectory;
    private readonly TranslationProjectStore _projectStore;

    /// <summary>Initialise le module.</summary>
    /// <param name="applicationDirectory">Dossier de l'application (pour localiser les tables de caractères prédéfinies).</param>
    /// <param name="projectStore">Magasin utilisé pour charger les projets Saturn.</param>
    public SaturnConsoleModule(string applicationDirectory, TranslationProjectStore projectStore)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationDirectory);
        ArgumentNullException.ThrowIfNull(projectStore);

        _applicationDirectory = applicationDirectory;
        _projectStore = projectStore;

        RomLoader = new Disc.SaturnRomLoader();
        RomValidator = new Disc.SaturnRomValidator();
        TextExtractor = new TextExtraction.SaturnTextExtractor();
        TextInjector = new SaturnTextInjector();
    }

    /// <inheritdoc />
    public string Id => ModuleId;

    /// <inheritdoc />
    public string DisplayName => Strings.Saturn_ModuleDisplayName;

    /// <inheritdoc />
    public string Description => Strings.Saturn_ModuleDescription;

    /// <inheritdoc />
    public string IconKey => "gamepad";

    /// <inheritdoc />
    public string Version => typeof(SaturnConsoleModule).Assembly.GetName().Version?.ToString() ?? "0.0.0";

    /// <inheritdoc />
    public IRomLoader RomLoader { get; }

    /// <inheritdoc />
    public IRomValidator RomValidator { get; }

    /// <inheritdoc />
    public ITextExtractor TextExtractor { get; }

    /// <inheritdoc />
    public ITextInjector TextInjector { get; }

    /// <inheritdoc />
    public ConsoleProjectContext LoadProjectContext(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        TranslationProject project = _projectStore.Load(projectPath);
        SaturnCharacterTable characterTable = ResolveCharacterTable(project.CharacterTableName);

        return new ConsoleProjectContext(project, characterTable, new SaturnTranslationLengthPolicy());
    }

    /// <summary>
    /// Résout la table de caractères d'un projet : une des tables prédéfinies si le nom correspond à l'une
    /// d'elles, sinon un fichier <c>.tbl</c> externe désigné par un chemin absolu.
    /// </summary>
    /// <param name="characterTableName"><see cref="TranslationProject.CharacterTableName" /> du projet.</param>
    private SaturnCharacterTable ResolveCharacterTable(string? characterTableName)
    {
        if (string.IsNullOrEmpty(characterTableName) || characterTableName == PredefinedCharacterTables.AsciiFileName)
        {
            return PredefinedCharacterTables.LoadAscii(_applicationDirectory);
        }

        if (characterTableName == PredefinedCharacterTables.ShiftJisKanaFileName)
        {
            return PredefinedCharacterTables.LoadShiftJisKana(_applicationDirectory);
        }

        CharacterTableFile file = CharacterTableFileReader.Read(characterTableName);
        return new SaturnCharacterTable(Path.GetFileNameWithoutExtension(characterTableName), file);
    }
}
