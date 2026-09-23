// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.Core.Editing;

/// <summary>Une modification réversible, empilée sur un <see cref="UndoRedoStack" />.</summary>
public interface IEditCommand
{
    /// <summary>Applique la modification.</summary>
    void Do();

    /// <summary>Annule la modification, en restaurant exactement l'état précédent.</summary>
    void Undo();
}
