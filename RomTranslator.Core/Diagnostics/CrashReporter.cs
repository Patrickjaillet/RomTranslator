// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using RomTranslator.Core.Portability;

namespace RomTranslator.Core.Diagnostics;

/// <summary>Écrit un rapport d'erreur fatale dans le dossier des journaux de l'application.</summary>
public static class CrashReporter
{
    private static readonly UTF8Encoding _utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>Tente d'écrire un rapport pour une exception non gérée.</summary>
    /// <param name="locations">Emplacements portables.</param>
    /// <param name="exception">Exception à consigner.</param>
    /// <param name="now">Horodatage à utiliser (heure locale courante par défaut).</param>
    /// <returns>Le chemin du rapport, ou <see langword="null" /> si l'écriture est impossible.</returns>
    public static string? TryWrite(PortableLocations locations, Exception exception, DateTimeOffset? now = null)
    {
        ArgumentNullException.ThrowIfNull(locations);
        ArgumentNullException.ThrowIfNull(exception);

        DateTimeOffset moment = now ?? DateTimeOffset.Now;
        string fileName = "crash-" + moment.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture) + ".log";

        try
        {
            Directory.CreateDirectory(locations.LogsDirectory);

            string path = Path.Combine(locations.LogsDirectory, fileName);
            StringBuilder report = new();
            report.AppendLine("RomTranslator - rapport d'erreur");
            report.AppendLine("Date : " + moment.ToString("O", CultureInfo.InvariantCulture));
            report.AppendLine("Système : " + RuntimeInformation.OSDescription);
            report.AppendLine("Runtime : " + RuntimeInformation.FrameworkDescription);
            report.AppendLine();
            report.AppendLine(exception.ToString());

            File.WriteAllText(path, report.ToString(), _utf8WithoutBom);
            return path;
        }
        catch (Exception writeFailure) when (writeFailure is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
