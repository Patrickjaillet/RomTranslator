// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.App.Services;

/// <summary>Ouvre un dossier local dans l'explorateur de fichiers de Windows.</summary>
public interface IFolderLauncher
{
    /// <summary>Tente d'ouvrir le dossier indiqué.</summary>
    /// <param name="path">Chemin absolu du dossier.</param>
    /// <returns><see langword="true" /> si le dossier a été transmis à Windows, sinon <see langword="false" />.</returns>
    bool TryOpen(string path);
}
