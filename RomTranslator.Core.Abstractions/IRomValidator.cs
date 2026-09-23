// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;

namespace RomTranslator.Core.Abstractions;

/// <summary>Résultat de la validation d'une image ROM.</summary>
/// <param name="IsValid">Indique si l'image est valide et exploitable.</param>
/// <param name="Messages">Messages d'information, d'avertissement ou d'erreur (déjà traduits) décrivant le résultat.</param>
public sealed record RomValidationResult(bool IsValid, IReadOnlyList<string> Messages);

/// <summary>Vérifie l'intégrité et la compatibilité d'une image ROM propre à une console.</summary>
public interface IRomValidator
{
    /// <summary>
    /// Vérifie l'intégrité de l'image (cohérence des fichiers d'une image multi-fichiers, checksum,
    /// région, format) sans en modifier le contenu.
    /// </summary>
    /// <param name="romPath">Chemin de l'image ROM (ou de son fichier principal, par exemple un <c>.cue</c>).</param>
    RomValidationResult Validate(string romPath);
}
