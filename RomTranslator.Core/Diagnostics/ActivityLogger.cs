// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using RomTranslator.Core.Portability;

namespace RomTranslator.Core.Diagnostics;

/// <summary>
/// Journal d'activité de l'application, écrit dans le dossier portable des journaux
/// (<c>logs/activity-AAAA-MM-JJ.log</c>, un fichier par jour). Le niveau de verbosité minimal est
/// configurable ; les messages en dessous de ce niveau ne sont pas écrits. Les fichiers les plus anciens
/// au-delà du nombre de jours conservés sont supprimés à l'initialisation.
/// </summary>
public sealed class ActivityLogger
{
    /// <summary>Nombre de fichiers journaliers conservés par défaut.</summary>
    public const int DefaultRetainedDays = 14;

    private static readonly UTF8Encoding _utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly object _writeLock = new();

    private readonly PortableLocations _locations;
    private readonly int _retainedDays;

    /// <summary>Initialise le journal d'activité.</summary>
    /// <param name="locations">Emplacements portables (dossier <c>logs/</c>).</param>
    /// <param name="minimumLevel">Niveau de verbosité minimal écrit dans le journal.</param>
    /// <param name="retainedDays">Nombre de fichiers journaliers conservés ; les plus anciens sont supprimés.</param>
    public ActivityLogger(PortableLocations locations, LogLevel minimumLevel = LogLevel.Information, int retainedDays = DefaultRetainedDays)
    {
        ArgumentNullException.ThrowIfNull(locations);
        if (retainedDays < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(retainedDays), retainedDays, "Au moins un jour doit être conservé.");
        }

        _locations = locations;
        MinimumLevel = minimumLevel;
        _retainedDays = retainedDays;
    }

    /// <summary>Niveau de verbosité minimal écrit dans le journal.</summary>
    public LogLevel MinimumLevel { get; set; }

    /// <summary>Écrit un message dans le journal du jour, si son niveau atteint <see cref="MinimumLevel" />.</summary>
    /// <param name="level">Niveau du message.</param>
    /// <param name="message">Message à consigner.</param>
    public void Log(LogLevel level, string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (level < MinimumLevel)
        {
            return;
        }

        string line = string.Format(
            CultureInfo.InvariantCulture, "[{0:O}] [{1}] {2}", DateTimeOffset.Now, level.ToString().ToUpperInvariant(), message);

        try
        {
            lock (_writeLock)
            {
                Directory.CreateDirectory(_locations.LogsDirectory);
                File.AppendAllLines(CurrentLogFilePath, new[] { line }, _utf8WithoutBom);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // L'échec d'écriture du journal ne doit jamais interrompre l'application.
        }
    }

    /// <summary>Chemin du fichier journal du jour courant.</summary>
    public string CurrentLogFilePath =>
        Path.Combine(_locations.LogsDirectory, "activity-" + DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".log");

    /// <summary>
    /// Supprime les fichiers journaliers d'activité au-delà du nombre de jours conservés, du plus ancien au
    /// plus récent. Sans effet si le dossier des journaux n'existe pas encore.
    /// </summary>
    public void PruneOldLogFiles()
    {
        if (!Directory.Exists(_locations.LogsDirectory))
        {
            return;
        }

        var excess = Directory.EnumerateFiles(_locations.LogsDirectory, "activity-*.log")
            .OrderByDescending(file => file, StringComparer.Ordinal)
            .Skip(_retainedDays);

        foreach (string file in excess)
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                // Un fichier verrouillé ou déjà supprimé ne doit pas interrompre le nettoyage des autres.
            }
        }
    }
}
