// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using RomTranslator.Core.Localization;

namespace RomTranslator.Modules.SegaSaturn.Disc;

/// <summary>
/// Analyse un fichier <c>.cue</c> (feuille de montage BIN/CUE) pour retrouver ses pistes et le fichier de
/// données associé à chacune. Ne prend en charge que la syntaxe couramment produite pour les images Saturn
/// (une ligne <c>FILE</c> par piste ou partagée entre pistes consécutives, une ligne <c>TRACK</c> par piste).
/// </summary>
public static class CueSheetReader
{
    /// <summary>Lit et analyse un fichier <c>.cue</c>.</summary>
    /// <param name="cuePath">Chemin du fichier <c>.cue</c>.</param>
    /// <exception cref="InvalidDataException">Le fichier ne contient aucune piste reconnaissable.</exception>
    public static CueSheet Read(string cuePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cuePath);

        string directory = Path.GetDirectoryName(Path.GetFullPath(cuePath)) ?? string.Empty;
        List<CueTrack> tracks = new();
        string? currentDataFilePath = null;

        foreach (string rawLine in File.ReadLines(cuePath))
        {
            string line = rawLine.Trim();

            if (line.StartsWith("FILE ", StringComparison.OrdinalIgnoreCase))
            {
                string fileName = ExtractQuoted(line)
                    ?? throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.Saturn_Error_CueMissingFileName, rawLine));
                currentDataFilePath = Path.Combine(directory, fileName);
            }
            else if (line.StartsWith("TRACK ", StringComparison.OrdinalIgnoreCase))
            {
                if (currentDataFilePath is null)
                {
                    throw new InvalidDataException(Strings.Saturn_Error_CueTrackBeforeFile);
                }

                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3 || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
                {
                    throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.Saturn_Error_CueMalformedTrackLine, rawLine));
                }

                CueTrackMode mode = ParseTrackMode(parts[2]);
                tracks.Add(new CueTrack(number, mode, currentDataFilePath));
            }
        }

        if (tracks.Count == 0)
        {
            throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.Saturn_Error_CueFileHasNoTrack, cuePath));
        }

        return new CueSheet(tracks);
    }

    private static CueTrackMode ParseTrackMode(string token)
    {
        return token.ToUpperInvariant() switch
        {
            "AUDIO" => CueTrackMode.Audio,
            "MODE1/2048" => CueTrackMode.Mode1Cooked,
            "MODE1/2352" => CueTrackMode.Mode1Raw,
            "MODE2/2336" or "MODE2/2352" => CueTrackMode.Mode2,
            _ => throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.Saturn_Error_CueUnsupportedTrackMode, token)),
        };
    }

    private static string? ExtractQuoted(string line)
    {
        int start = line.IndexOf('"', StringComparison.Ordinal);
        if (start < 0)
        {
            return null;
        }

        int end = line.IndexOf('"', start + 1);
        return end < 0 ? null : line[(start + 1)..end];
    }
}
