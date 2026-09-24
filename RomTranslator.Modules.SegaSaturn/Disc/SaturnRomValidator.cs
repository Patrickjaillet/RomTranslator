// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Localization;

namespace RomTranslator.Modules.SegaSaturn.Disc;

/// <summary>
/// Vérifie l'intégrité d'une image disque Sega Saturn (BIN/CUE) : présence et cohérence de taille des
/// fichiers référencés par la feuille de montage, et présence de l'identifiant matériel Saturn en tête de
/// la piste de données. Ne modifie jamais l'image contrôlée.
/// </summary>
public sealed class SaturnRomValidator : IRomValidator
{
    private static readonly CompositeFormat TrackFileMissingFormat = CompositeFormat.Parse(Strings.Saturn_Validation_TrackFileMissing);
    private static readonly CompositeFormat NoDataTrackFormat = CompositeFormat.Parse(Strings.Saturn_Error_NoDataTrack);
    private static readonly CompositeFormat MissingHardwareIdFormat = CompositeFormat.Parse(Strings.Saturn_Error_MissingHardwareId);
    private static readonly CompositeFormat DetectedRegionFormat = CompositeFormat.Parse(Strings.Saturn_Validation_DetectedRegion);
    private static readonly CompositeFormat UnreadableIpBinFormat = CompositeFormat.Parse(Strings.Saturn_Validation_UnreadableIpBin);
    private static readonly CompositeFormat TruncatedImageFormat = CompositeFormat.Parse(Strings.Saturn_Validation_TruncatedImage);

    /// <inheritdoc />
    public RomValidationResult Validate(string romPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);

        List<string> errors = new();
        List<string> infos = new();

        CueSheet cueSheet;
        try
        {
            cueSheet = CueSheetReader.Read(romPath);
        }
        catch (InvalidDataException exception)
        {
            return new RomValidationResult(false, new[] { exception.Message });
        }

        foreach (CueTrack track in cueSheet.Tracks)
        {
            if (!File.Exists(track.DataFilePath))
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, TrackFileMissingFormat, track.Number, track.DataFilePath));
            }
        }

        CueTrack? dataTrack = cueSheet.FirstDataTrack;
        if (dataTrack is null)
        {
            errors.Add(string.Format(CultureInfo.CurrentCulture, NoDataTrackFormat, romPath));
        }

        if (errors.Count > 0)
        {
            return new RomValidationResult(false, errors);
        }

        if (!TrackSizeIsConsistent(dataTrack!, out string? sizeError))
        {
            errors.Add(sizeError!);
        }

        try
        {
            using SectorReader sectorReader = new(dataTrack!);
            byte[] headerBytes = sectorReader.ReadBytes(startSector: 0, byteLength: SaturnIpBin.HeaderRegionSize);
            SaturnIpBinHeader header = SaturnIpBin.Parse(headerBytes);

            if (!string.Equals(header.HardwareId.TrimEnd(), SaturnIpBin.ExpectedHardwareId.TrimEnd(), StringComparison.Ordinal))
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, MissingHardwareIdFormat, romPath));
            }
            else
            {
                string region = string.IsNullOrEmpty(header.AreaSymbols) ? Strings.Saturn_Validation_UnknownRegion : header.AreaSymbols;
                infos.Add(string.Format(CultureInfo.CurrentCulture, DetectedRegionFormat, region));
            }
        }
        catch (Exception exception) when (exception is InvalidDataException or EndOfStreamException or IOException)
        {
            errors.Add(string.Format(CultureInfo.CurrentCulture, UnreadableIpBinFormat, exception.Message));
        }

        List<string> messages = new(errors.Count + infos.Count);
        messages.AddRange(errors);
        messages.AddRange(infos);

        return new RomValidationResult(errors.Count == 0, messages);
    }

    private static bool TrackSizeIsConsistent(CueTrack track, out string? message)
    {
        long fileLength = new FileInfo(track.DataFilePath).Length;
        int sectorStride = track.Mode switch
        {
            CueTrackMode.Mode1Cooked => SectorReader.SectorDataSize,
            CueTrackMode.Mode1Raw or CueTrackMode.Mode2 => 2352,
            _ => SectorReader.SectorDataSize,
        };

        if (fileLength % sectorStride != 0)
        {
            message = string.Format(CultureInfo.CurrentCulture, TruncatedImageFormat, fileLength, sectorStride);
            return false;
        }

        message = null;
        return true;
    }
}
