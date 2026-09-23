// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.Core.Projects;

namespace RomTranslator.Core.Editing;

/// <summary>Modifie le texte traduit d'une entrée.</summary>
public sealed class EditTranslatedTextCommand : IEditCommand
{
    private readonly TranslationEntry _entry;
    private readonly string _previousText;
    private readonly string _newText;

    /// <summary>Initialise la modification.</summary>
    /// <param name="entry">Entrée à modifier.</param>
    /// <param name="newText">Nouveau texte traduit.</param>
    public EditTranslatedTextCommand(TranslationEntry entry, string newText)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(newText);

        _entry = entry;
        _previousText = entry.TranslatedText;
        _newText = newText;
    }

    /// <inheritdoc />
    public void Do()
    {
        _entry.TranslatedText = _newText;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _entry.TranslatedText = _previousText;
    }
}
