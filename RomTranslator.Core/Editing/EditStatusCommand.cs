// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Projects;

namespace RomTranslator.Core.Editing;

/// <summary>Modifie le statut de traduction d'une entrée.</summary>
public sealed class EditStatusCommand : IEditCommand
{
    private readonly TranslationEntry _entry;
    private readonly TranslationStatus _previousStatus;
    private readonly TranslationStatus _newStatus;

    /// <summary>Initialise la modification.</summary>
    /// <param name="entry">Entrée à modifier.</param>
    /// <param name="newStatus">Nouveau statut.</param>
    public EditStatusCommand(TranslationEntry entry, TranslationStatus newStatus)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _entry = entry;
        _previousStatus = entry.Status;
        _newStatus = newStatus;
    }

    /// <inheritdoc />
    public void Do()
    {
        _entry.Status = _newStatus;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _entry.Status = _previousStatus;
    }
}
