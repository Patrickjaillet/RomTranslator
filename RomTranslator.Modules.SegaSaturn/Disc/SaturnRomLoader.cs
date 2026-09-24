// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.IO;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Localization;

namespace RomTranslator.Modules.SegaSaturn.Disc;

/// <summary>Charge une image disque Sega Saturn (BIN/CUE) et lit ses métadonnées depuis l'en-tête IP.BIN.</summary>
public sealed class SaturnRomLoader : IRomLoader
{
    /// <inheritdoc />
    /// <exception cref="InvalidDataException">
    /// Le fichier <c>.cue</c> ne décrit aucune piste de données, ou l'en-tête IP.BIN ne commence pas par
    /// l'identifiant matériel attendu.
    /// </exception>
    public RomMetadata Load(string romPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);

        CueSheet cueSheet = CueSheetReader.Read(romPath);
        CueTrack dataTrack = cueSheet.FirstDataTrack
            ?? throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.Saturn_Error_NoDataTrack, romPath));

        using SectorReader sectorReader = new(dataTrack);
        byte[] headerBytes = sectorReader.ReadBytes(startSector: 0, byteLength: SaturnIpBin.HeaderRegionSize);

        SaturnIpBinHeader header = SaturnIpBin.Parse(headerBytes);
        if (!string.Equals(header.HardwareId.TrimEnd(), SaturnIpBin.ExpectedHardwareId.TrimEnd(), StringComparison.Ordinal))
        {
            throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.Saturn_Error_MissingHardwareId, romPath));
        }

        return new RomMetadata(
            Title: header.GameTitle,
            Publisher: string.IsNullOrEmpty(header.MakerId) ? null : header.MakerId,
            Region: string.IsNullOrEmpty(header.AreaSymbols) ? null : header.AreaSymbols,
            ProductId: string.IsNullOrEmpty(header.ProductNumber) ? null : header.ProductNumber);
    }
}
