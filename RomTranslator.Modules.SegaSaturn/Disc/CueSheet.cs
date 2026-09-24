// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;
using System.Linq;

namespace RomTranslator.Modules.SegaSaturn.Disc;

/// <summary>Mode d'une piste de données décrite par un fichier <c>.cue</c>.</summary>
public enum CueTrackMode
{
    /// <summary>Piste audio (CD-DA), sans intérêt pour le chargement d'une image Saturn.</summary>
    Audio,

    /// <summary>Secteurs de 2 048 octets ne contenant que les données utiles (déjà extraites du secteur brut).</summary>
    Mode1Cooked,

    /// <summary>Secteurs bruts de 2 352 octets (en-tête de synchronisation de 16 octets suivi de 2 048 octets de données utiles pour le Mode 1).</summary>
    Mode1Raw,

    /// <summary>Secteurs Mode 2 (Form 1/Form 2), normalisés à 2 048 octets de données utiles par l'outil de lecture.</summary>
    Mode2,
}

/// <summary>Une piste décrite par un fichier <c>.cue</c>.</summary>
/// <param name="Number">Numéro de piste (1-based).</param>
/// <param name="Mode">Mode de la piste.</param>
/// <param name="DataFilePath">Chemin absolu du fichier de données associé à cette piste.</param>
public sealed record CueTrack(int Number, CueTrackMode Mode, string DataFilePath);

/// <summary>Résultat de l'analyse d'un fichier <c>.cue</c> : ses pistes, dans l'ordre du fichier.</summary>
public sealed record CueSheet(IReadOnlyList<CueTrack> Tracks)
{
    /// <summary>Première piste de données (non audio) de la feuille de montage, ou <see langword="null" /> si absente.</summary>
    public CueTrack? FirstDataTrack => Tracks.FirstOrDefault(track => track.Mode != CueTrackMode.Audio);
}
