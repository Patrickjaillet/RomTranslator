// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RomTranslator.Core.Binary;
using RomTranslator.Core.Localization;

namespace RomTranslator.App.ViewModels;

/// <summary>Une plage d'octets qui diffère, prête à l'affichage (décalage et octets en hexadécimal).</summary>
public sealed class BinaryDifferenceViewModel
{
    internal BinaryDifferenceViewModel(BinaryDifference difference)
    {
        OffsetDisplay = "0x" + difference.Offset.ToString("X8", CultureInfo.InvariantCulture);
        LengthInBytes = difference.Length;
        OriginalHex = string.Join(' ', Array.ConvertAll(difference.OriginalBytes, b => b.ToString("X2", CultureInfo.InvariantCulture)));
        ModifiedHex = string.Join(' ', Array.ConvertAll(difference.ModifiedBytes, b => b.ToString("X2", CultureInfo.InvariantCulture)));
    }

    /// <summary>Décalage de début de la plage, en hexadécimal.</summary>
    public string OffsetDisplay { get; }

    /// <summary>Longueur de la plage, en octets.</summary>
    public int LengthInBytes { get; }

    /// <summary>Octets de l'image originale, en hexadécimal.</summary>
    public string OriginalHex { get; }

    /// <summary>Octets de l'image modifiée, en hexadécimal.</summary>
    public string ModifiedHex { get; }
}

/// <summary>
/// Comparaison de deux images ROM (originale et modifiée) : liste des plages d'octets qui diffèrent,
/// avec la possibilité de générer un patch IPS à partir du résultat.
/// </summary>
public sealed class BinaryDiffViewModel
{
    /// <summary>Compare deux fichiers et prépare le résultat pour l'affichage.</summary>
    /// <param name="originalPath">Chemin de l'image ROM d'origine.</param>
    /// <param name="modifiedPath">Chemin de l'image ROM modifiée.</param>
    public BinaryDiffViewModel(string originalPath, string modifiedPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originalPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(modifiedPath);

        OriginalPath = originalPath;
        ModifiedPath = modifiedPath;

        IReadOnlyList<BinaryDifference> differences = BinaryComparer.Compare(originalPath, modifiedPath);
        Differences = differences.Select(difference => new BinaryDifferenceViewModel(difference)).ToList();
        SummaryText = string.Format(CultureInfo.CurrentCulture, Strings.Diff_Summary, Differences.Count);
    }

    /// <summary>Chemin de l'image ROM d'origine.</summary>
    public string OriginalPath { get; }

    /// <summary>Chemin de l'image ROM modifiée.</summary>
    public string ModifiedPath { get; }

    /// <summary>Plages d'octets qui diffèrent, dans l'ordre des décalages.</summary>
    public IReadOnlyList<BinaryDifferenceViewModel> Differences { get; }

    /// <summary>Résumé du nombre de différences trouvées.</summary>
    public string SummaryText { get; }

    /// <summary>Indique si les deux images sont identiques.</summary>
    public bool AreIdentical => Differences.Count == 0;

    /// <summary>Génère un patch IPS à partir de cette comparaison.</summary>
    /// <param name="patchPath">Chemin du fichier patch <c>.ips</c> à créer.</param>
    /// <exception cref="NotSupportedException">Une différence dépasse la limite de 16 Mio du format IPS.</exception>
    public void CreateIpsPatch(string patchPath)
    {
        IpsPatch.Create(OriginalPath, ModifiedPath, patchPath);
    }
}
