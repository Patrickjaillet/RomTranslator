// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.Core.Abstractions;

/// <summary>Métadonnées d'un jeu lues dans son image ROM.</summary>
/// <param name="Title">Titre du jeu.</param>
/// <param name="Publisher">Éditeur, ou <see langword="null" /> si inconnu.</param>
/// <param name="Region">Région (par exemple « Japon », « États-Unis », « Europe »), ou <see langword="null" /> si inconnue.</param>
/// <param name="ProductId">Identifiant produit propre à la console, ou <see langword="null" /> si inconnu.</param>
public sealed record RomMetadata(string Title, string? Publisher, string? Region, string? ProductId);

/// <summary>Charge et expose le contenu d'une image ROM propre à une console.</summary>
public interface IRomLoader
{
    /// <summary>
    /// Charge une image ROM et en lit les métadonnées.
    /// </summary>
    /// <param name="romPath">Chemin de l'image ROM (ou de son fichier principal, par exemple un <c>.cue</c>).</param>
    /// <exception cref="System.IO.InvalidDataException">L'image ne correspond pas au format attendu par la console.</exception>
    RomMetadata Load(string romPath);
}
