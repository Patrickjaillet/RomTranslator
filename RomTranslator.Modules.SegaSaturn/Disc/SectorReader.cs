// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;

namespace RomTranslator.Modules.SegaSaturn.Disc;

/// <summary>
/// Lit les 2 048 octets de données utiles d'un secteur de la piste de données d'une image disque, quel que
/// soit le mode physique du secteur sur le fichier source (secteurs déjà réduits à 2 048 octets, ou secteurs
/// bruts de 2 352 octets avec en-tête de synchronisation à ignorer).
/// </summary>
public sealed class SectorReader : IDisposable
{
    /// <summary>Taille des données utiles d'un secteur, quel que soit son mode physique.</summary>
    public const int SectorDataSize = 2048;

    private const int RawSectorSize = 2352;
    private const int RawSectorHeaderSize = 16;

    private readonly FileStream _stream;
    private readonly int _sectorStride;
    private readonly int _dataOffsetInSector;

    /// <summary>Ouvre un lecteur de secteurs pour la piste de données indiquée.</summary>
    /// <param name="track">Piste de données à lire (une piste audio n'a pas de sens ici).</param>
    /// <exception cref="ArgumentException"><paramref name="track" /> est une piste audio.</exception>
    public SectorReader(CueTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);

        if (track.Mode == CueTrackMode.Audio)
        {
            throw new ArgumentException("Une piste audio n'a pas de secteurs de données à lire.", nameof(track));
        }

        (_sectorStride, _dataOffsetInSector) = track.Mode switch
        {
            CueTrackMode.Mode1Cooked => (SectorDataSize, 0),
            CueTrackMode.Mode1Raw => (RawSectorSize, RawSectorHeaderSize),
            CueTrackMode.Mode2 => (RawSectorSize, RawSectorHeaderSize),
            _ => throw new ArgumentOutOfRangeException(nameof(track), track.Mode, "Mode de piste non pris en charge."),
        };

        _stream = File.OpenRead(track.DataFilePath);
    }

    /// <summary>Lit les 2 048 octets de données utiles d'un secteur.</summary>
    /// <param name="sectorIndex">Index du secteur (0-based) au sein de la piste.</param>
    public byte[] ReadSector(long sectorIndex)
    {
        if (sectorIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sectorIndex), sectorIndex, "L'index de secteur ne peut pas être négatif.");
        }

        long filePosition = (sectorIndex * _sectorStride) + _dataOffsetInSector;
        _stream.Seek(filePosition, SeekOrigin.Begin);

        byte[] buffer = new byte[SectorDataSize];
        int totalRead = 0;
        while (totalRead < SectorDataSize)
        {
            int read = _stream.Read(buffer, totalRead, SectorDataSize - totalRead);
            if (read == 0)
            {
                throw new EndOfStreamException($"Fin de fichier atteinte en lisant le secteur {sectorIndex}.");
            }

            totalRead += read;
        }

        return buffer;
    }

    /// <summary>Lit une plage contiguë d'octets couvrant un ou plusieurs secteurs, à partir d'un décalage logique.</summary>
    /// <param name="startSector">Premier secteur à lire.</param>
    /// <param name="byteLength">Nombre d'octets à lire, depuis le début du premier secteur.</param>
    public byte[] ReadBytes(long startSector, int byteLength)
    {
        if (byteLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteLength), byteLength, "La longueur ne peut pas être négative.");
        }

        int sectorCount = (byteLength + SectorDataSize - 1) / SectorDataSize;
        sectorCount = Math.Max(sectorCount, 1);

        byte[] result = new byte[byteLength];
        int written = 0;

        for (int i = 0; i < sectorCount && written < byteLength; i++)
        {
            byte[] sector = ReadSector(startSector + i);
            int toCopy = Math.Min(SectorDataSize, byteLength - written);
            Array.Copy(sector, 0, result, written, toCopy);
            written += toCopy;
        }

        return result;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _stream.Dispose();
    }
}
