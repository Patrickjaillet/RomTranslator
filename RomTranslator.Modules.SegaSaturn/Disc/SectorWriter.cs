// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.IO;

namespace RomTranslator.Modules.SegaSaturn.Disc;

/// <summary>
/// Écrit des octets dans les 2 048 octets de données utiles d'un secteur de la piste de données d'une image
/// disque, en tenant compte du même décalage d'en-tête que <see cref="SectorReader" /> selon le mode physique
/// de la piste. N'écrit jamais en dehors de la zone de données utiles d'un secteur (l'en-tête de
/// synchronisation d'un secteur brut n'est jamais modifié).
/// </summary>
public sealed class SectorWriter : IDisposable
{
    private const int RawSectorSize = 2352;
    private const int RawSectorHeaderSize = 16;

    private readonly FileStream _stream;
    private readonly int _sectorStride;
    private readonly int _dataOffsetInSector;

    /// <summary>Ouvre un lecteur/écrivain de secteurs pour la piste de données indiquée.</summary>
    /// <param name="track">Piste de données à modifier (une piste audio n'a pas de sens ici).</param>
    /// <exception cref="ArgumentException"><paramref name="track" /> est une piste audio.</exception>
    public SectorWriter(CueTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);

        if (track.Mode == CueTrackMode.Audio)
        {
            throw new ArgumentException("Une piste audio n'a pas de secteurs de données à écrire.", nameof(track));
        }

        (_sectorStride, _dataOffsetInSector) = track.Mode switch
        {
            CueTrackMode.Mode1Cooked => (SectorReader.SectorDataSize, 0),
            CueTrackMode.Mode1Raw => (RawSectorSize, RawSectorHeaderSize),
            CueTrackMode.Mode2 => (RawSectorSize, RawSectorHeaderSize),
            _ => throw new ArgumentOutOfRangeException(nameof(track), track.Mode, "Mode de piste non pris en charge."),
        };

        _stream = new FileStream(track.DataFilePath, FileMode.Open, FileAccess.ReadWrite);
    }

    /// <summary>
    /// Écrit une plage contiguë d'octets à partir d'un décalage logique (0-based, dans les données utiles
    /// concaténées de la piste, comme <see cref="SectorReader.ReadBytes" />), sans jamais dépasser la limite
    /// de 2 048 octets utiles d'un secteur pour chaque segment écrit.
    /// </summary>
    /// <param name="logicalOffset">Décalage logique, dans les données utiles concaténées de la piste.</param>
    /// <param name="bytes">Octets à écrire.</param>
    public void WriteBytes(long logicalOffset, IReadOnlyList<byte> bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        int written = 0;
        while (written < bytes.Count)
        {
            long currentLogicalOffset = logicalOffset + written;
            long sectorIndex = currentLogicalOffset / SectorReader.SectorDataSize;
            int offsetInSector = (int)(currentLogicalOffset - (sectorIndex * SectorReader.SectorDataSize));
            int lengthInSector = Math.Min(bytes.Count - written, SectorReader.SectorDataSize - offsetInSector);

            long filePosition = (sectorIndex * _sectorStride) + _dataOffsetInSector + offsetInSector;
            _stream.Seek(filePosition, SeekOrigin.Begin);

            for (int i = 0; i < lengthInSector; i++)
            {
                _stream.WriteByte(bytes[written + i]);
            }

            written += lengthInSector;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _stream.Dispose();
    }
}
