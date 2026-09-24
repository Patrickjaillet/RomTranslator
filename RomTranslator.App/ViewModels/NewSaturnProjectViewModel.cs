// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Localization;
using RomTranslator.Core.Projects;
using RomTranslator.Modules.SegaSaturn;
using RomTranslator.Modules.SegaSaturn.CharacterTables;

namespace RomTranslator.App.ViewModels;

/// <summary>Table de caractères proposée par l'assistant de création d'un projet Saturn.</summary>
public enum SaturnCharacterTableChoice
{
    /// <summary>Table ASCII prédéfinie.</summary>
    Ascii,

    /// <summary>Table Shift-JIS (hiragana/katakana) prédéfinie.</summary>
    ShiftJisKana,

    /// <summary>Fichier <c>.tbl</c> externe, choisi par l'utilisateur.</summary>
    ExternalFile,
}

/// <summary>
/// Étape courante de l'assistant de création d'un projet Saturn : sélection de l'image disque, puis
/// paramètres du projet, puis création effective.
/// </summary>
public enum NewSaturnProjectStep
{
    /// <summary>Sélection et validation de l'image disque source.</summary>
    SelectImage,

    /// <summary>Nom du projet et choix de la table de caractères.</summary>
    ProjectSettings,

    /// <summary>Création terminée : le projet est enregistré et prêt à être ouvert.</summary>
    Done,
}

/// <summary>
/// Assistant de création d'un projet de traduction Sega Saturn : sélection de l'image disque (validée par
/// <see cref="IRomValidator" />), nom du projet et table de caractères, puis extraction automatique du texte
/// et enregistrement du fichier <c>.rtproj</c>.
/// </summary>
public sealed class NewSaturnProjectViewModel : ObservableObject
{
    private static readonly CompositeFormat CreationFailedFormat = CompositeFormat.Parse(Strings.NewProject_CreationFailed);

    private readonly SaturnConsoleModule _module;
    private readonly TranslationProjectStore _projectStore;
    private readonly string _applicationDirectory;
    private readonly string _projectsDirectory;

    private NewSaturnProjectStep _step = NewSaturnProjectStep.SelectImage;
    private string _romPath = string.Empty;
    private bool _isRomValid;
    private IReadOnlyList<string> _validationMessages = Array.Empty<string>();
    private string _projectName = string.Empty;
    private SaturnCharacterTableChoice _characterTableChoice = SaturnCharacterTableChoice.Ascii;
    private string? _externalCharacterTablePath;
    private string? _errorMessage;
    private string? _createdProjectPath;

    /// <summary>Initialise l'assistant.</summary>
    /// <param name="module">Module Saturn utilisé pour valider l'image et extraire le texte.</param>
    /// <param name="projectStore">Magasin utilisé pour enregistrer le projet créé.</param>
    /// <param name="applicationDirectory">Dossier de l'application (tables de caractères prédéfinies).</param>
    /// <param name="projectsDirectory">Dossier portable où enregistrer les nouveaux projets par défaut.</param>
    public NewSaturnProjectViewModel(
        SaturnConsoleModule module, TranslationProjectStore projectStore, string applicationDirectory, string projectsDirectory)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(projectStore);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectsDirectory);

        _module = module;
        _projectStore = projectStore;
        _applicationDirectory = applicationDirectory;
        _projectsDirectory = projectsDirectory;

        ValidateImageCommand = new RelayCommand<string>(ValidateImage, path => !string.IsNullOrWhiteSpace(path));
        ContinueToSettingsCommand = new RelayCommand(() => Step = NewSaturnProjectStep.ProjectSettings, () => IsRomValid);
        BackToImageCommand = new RelayCommand(() => Step = NewSaturnProjectStep.SelectImage);
        CreateProjectCommand = new RelayCommand(CreateProject, CanCreateProject);
    }

    /// <summary>Étape courante de l'assistant.</summary>
    public NewSaturnProjectStep Step
    {
        get => _step;
        private set => SetProperty(ref _step, value);
    }

    /// <summary>Chemin de l'image disque (fichier <c>.cue</c>) choisie.</summary>
    public string RomPath
    {
        get => _romPath;
        set => SetProperty(ref _romPath, value ?? string.Empty);
    }

    /// <summary>Indique si la dernière validation de l'image a réussi.</summary>
    public bool IsRomValid
    {
        get => _isRomValid;
        private set
        {
            if (SetProperty(ref _isRomValid, value))
            {
                ContinueToSettingsCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>Messages produits par la validation de l'image (erreurs ou informations).</summary>
    public IReadOnlyList<string> ValidationMessages
    {
        get => _validationMessages;
        private set => SetProperty(ref _validationMessages, value);
    }

    /// <summary>Nom du projet à créer.</summary>
    public string ProjectName
    {
        get => _projectName;
        set
        {
            if (SetProperty(ref _projectName, value ?? string.Empty))
            {
                CreateProjectCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>Table de caractères choisie pour le projet.</summary>
    public SaturnCharacterTableChoice CharacterTableChoice
    {
        get => _characterTableChoice;
        set
        {
            if (SetProperty(ref _characterTableChoice, value))
            {
                CreateProjectCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>Chemin du fichier <c>.tbl</c> externe choisi, requis si <see cref="CharacterTableChoice" /> vaut <see cref="SaturnCharacterTableChoice.ExternalFile" />.</summary>
    public string? ExternalCharacterTablePath
    {
        get => _externalCharacterTablePath;
        set
        {
            if (SetProperty(ref _externalCharacterTablePath, value))
            {
                CreateProjectCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>Message d'erreur de la dernière tentative de création, ou <see langword="null" />.</summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>Chemin du fichier <c>.rtproj</c> créé, disponible une fois <see cref="Step" /> à <see cref="NewSaturnProjectStep.Done" />.</summary>
    public string? CreatedProjectPath
    {
        get => _createdProjectPath;
        private set => SetProperty(ref _createdProjectPath, value);
    }

    /// <summary>Valide l'image disque choisie et avance <see cref="IsRomValid" /> en conséquence.</summary>
    public RelayCommand<string> ValidateImageCommand { get; }

    /// <summary>Passe à l'étape des paramètres du projet.</summary>
    public RelayCommand ContinueToSettingsCommand { get; }

    /// <summary>Revient à l'étape de sélection de l'image.</summary>
    public RelayCommand BackToImageCommand { get; }

    /// <summary>Crée le projet : extraction automatique du texte puis enregistrement du fichier <c>.rtproj</c>.</summary>
    public RelayCommand CreateProjectCommand { get; }

    private void ValidateImage(string? path)
    {
        RomPath = path ?? string.Empty;

        RomValidationResult result = _module.RomValidator.Validate(RomPath);
        ValidationMessages = result.Messages;
        IsRomValid = result.IsValid;

        if (result.IsValid && string.IsNullOrWhiteSpace(ProjectName))
        {
            ProjectName = Path.GetFileNameWithoutExtension(RomPath);
        }
    }

    private bool CanCreateProject()
    {
        if (string.IsNullOrWhiteSpace(ProjectName) || !IsRomValid)
        {
            return false;
        }

        return CharacterTableChoice != SaturnCharacterTableChoice.ExternalFile || !string.IsNullOrWhiteSpace(ExternalCharacterTablePath);
    }

    private void CreateProject()
    {
        ErrorMessage = null;

        try
        {
            TranslationProject project = TranslationProjectFactory.Create(ProjectName, SaturnConsoleModule.ModuleId, RomPath);
            project.CharacterTableName = ResolveCharacterTableName();

            ICharacterTable characterTable = ResolveCharacterTable(project.CharacterTableName);
            project.Entries.AddRange(ExtractEntries(characterTable));

            string projectPath = Path.Combine(_projectsDirectory, MakeSafeFileName(ProjectName) + TranslationProjectStore.FileExtension);
            _projectStore.Save(project, projectPath);

            CreatedProjectPath = projectPath;
            Step = NewSaturnProjectStep.Done;
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            ErrorMessage = string.Format(CultureInfo.CurrentCulture, CreationFailedFormat, exception.Message);
        }
    }

    private List<Core.Projects.TranslationEntry> ExtractEntries(ICharacterTable characterTable)
    {
        List<Core.Projects.TranslationEntry> entries = new();
        foreach (ITranslationEntry entry in _module.TextExtractor.ExtractAutomatically(RomPath, characterTable))
        {
            entries.Add((Core.Projects.TranslationEntry)entry);
        }

        return entries;
    }

    private string? ResolveCharacterTableName()
    {
        return CharacterTableChoice switch
        {
            SaturnCharacterTableChoice.Ascii => PredefinedCharacterTables.AsciiFileName,
            SaturnCharacterTableChoice.ShiftJisKana => PredefinedCharacterTables.ShiftJisKanaFileName,
            SaturnCharacterTableChoice.ExternalFile => ExternalCharacterTablePath,
            _ => PredefinedCharacterTables.AsciiFileName,
        };
    }

    private SaturnCharacterTable ResolveCharacterTable(string? characterTableName)
    {
        return CharacterTableChoice switch
        {
            SaturnCharacterTableChoice.ShiftJisKana => PredefinedCharacterTables.LoadShiftJisKana(_applicationDirectory),
            SaturnCharacterTableChoice.ExternalFile => new SaturnCharacterTable(
                Path.GetFileNameWithoutExtension(characterTableName!), CharacterTableFileReader.Read(characterTableName!)),
            _ => PredefinedCharacterTables.LoadAscii(_applicationDirectory),
        };
    }

    private static string MakeSafeFileName(string name)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        char[] sanitized = name.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        string result = new string(sanitized).Trim();

        return string.IsNullOrEmpty(result) ? "projet" : result;
    }
}
