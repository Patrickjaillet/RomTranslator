// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.Core.Abstractions;

namespace RomTranslator.Modules.SegaSaturn.ViewModels;

/// <summary>
/// Présente les métadonnées d'un jeu Saturn détecté (<see cref="RomMetadata" />) sous une forme prête à
/// l'affichage. Les métadonnées d'une image ne changent jamais après chargement : ce modèle de vue est
/// immuable, sans notification de changement. Pas encore relié à un onglet réel de l'interface (aucun
/// mécanisme de découverte de modules n'existe encore, voir Phase 4.2 et <c>MEMOIRE.md</c>).
/// </summary>
public sealed class SaturnRomInfoViewModel
{
    /// <summary>Construit la présentation à partir des métadonnées lues par <see cref="Disc.SaturnRomLoader" />.</summary>
    /// <param name="metadata">Métadonnées de l'image chargée.</param>
    public SaturnRomInfoViewModel(RomMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        Title = metadata.Title;
        Publisher = metadata.Publisher;
        Region = metadata.Region;
        ProductId = metadata.ProductId;
    }

    /// <summary>Titre du jeu.</summary>
    public string Title { get; }

    /// <summary>Éditeur du jeu, ou <see langword="null" /> si inconnu (à représenter par la vue appelante, par exemple via une ressource i18n de substitution).</summary>
    public string? Publisher { get; }

    /// <summary>Région/zone cible du jeu, ou <see langword="null" /> si inconnue.</summary>
    public string? Region { get; }

    /// <summary>Identifiant produit du jeu, ou <see langword="null" /> si inconnu.</summary>
    public string? ProductId { get; }
}
