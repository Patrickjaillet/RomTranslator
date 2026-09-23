// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.App.Services;

/// <summary>Ouvre une adresse externe (site web, messagerie) avec l'application par défaut de Windows.</summary>
public interface IUrlLauncher
{
    /// <summary>Tente d'ouvrir l'adresse indiquée.</summary>
    /// <param name="url">Adresse absolue (http, https ou mailto).</param>
    /// <returns><see langword="true" /> si l'adresse a été transmise à Windows, sinon <see langword="false" />.</returns>
    bool TryOpen(string url);
}
