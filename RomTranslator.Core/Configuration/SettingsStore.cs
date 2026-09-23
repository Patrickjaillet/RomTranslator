// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using RomTranslator.Core.Portability;

namespace RomTranslator.Core.Configuration;

/// <summary>Lecture et écriture des paramètres de l'application dans le dossier portable.</summary>
public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly PortableLocations _locations;

    /// <summary>Initialise le magasin de paramètres.</summary>
    public SettingsStore(PortableLocations locations)
    {
        ArgumentNullException.ThrowIfNull(locations);

        _locations = locations;
    }

    /// <summary>
    /// Charge les paramètres. Retourne les valeurs par défaut si le fichier est absent ou illisible.
    /// Un fichier dont le contenu est invalide est conservé sous le nom <c>settings.json.corrupt-DATE</c>
    /// afin de ne pas être écrasé silencieusement.
    /// </summary>
    public AppSettings Load()
    {
        string path = _locations.SettingsFilePath;
        if (!File.Exists(path))
        {
            return new AppSettings();
        }

        try
        {
            using FileStream stream = File.OpenRead(path);
            return JsonSerializer.Deserialize<AppSettings>(stream, _jsonOptions) ?? new AppSettings();
        }
        catch (JsonException)
        {
            QuarantineCorruptFile(path);
            return new AppSettings();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new AppSettings();
        }
    }

    /// <summary>
    /// Enregistre les paramètres de façon atomique : écriture dans un fichier temporaire du même dossier,
    /// puis remplacement du fichier de destination.
    /// </summary>
    /// <exception cref="IOException">Le dossier de configuration n'est pas accessible en écriture.</exception>
    public void Save(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Directory.CreateDirectory(_locations.ConfigDirectory);

        string target = _locations.SettingsFilePath;
        string temporary = target + ".tmp";

        using (FileStream stream = File.Create(temporary))
        {
            JsonSerializer.Serialize(stream, settings, _jsonOptions);
        }

        File.Move(temporary, target, overwrite: true);
    }

    private static void QuarantineCorruptFile(string path)
    {
        string stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

        try
        {
            File.Move(path, path + ".corrupt-" + stamp, overwrite: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Le fichier reste en place ; il sera remplacé au prochain enregistrement.
        }
    }
}
