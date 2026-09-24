// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RomTranslator.Core.Projects;

/// <summary>
/// Exporte et importe les entrées de traduction d'un projet au format CSV, pour une relecture externe
/// dans un tableur (Excel, LibreOffice Calc). Colonnes : id, texte source, texte traduit, statut, contexte.
/// </summary>
public static class TranslationCsvExchange
{
    private static readonly string[] Header = { "id", "sourceText", "translatedText", "status", "context" };

    /// <summary>Exporte les entrées d'un projet vers un fichier CSV.</summary>
    /// <param name="entries">Entrées à exporter.</param>
    /// <param name="path">Chemin du fichier CSV à créer.</param>
    public static void Export(IReadOnlyList<TranslationEntry> entries, string path)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using StreamWriter writer = new(path, append: false, Encoding.UTF8);
        writer.WriteLine(string.Join(',', Header));

        foreach (TranslationEntry entry in entries)
        {
            writer.WriteLine(string.Join(',', new[]
            {
                Escape(entry.Id),
                Escape(entry.SourceText),
                Escape(entry.TranslatedText),
                Escape(entry.Status.ToString()),
                Escape(entry.Context ?? string.Empty),
            }));
        }
    }

    /// <summary>
    /// Importe des traductions relues depuis un fichier CSV et les applique aux entrées correspondantes
    /// (rapprochement par <c>id</c>). Les colonnes texte source, statut et contexte du fichier sont
    /// ignorées : seule la traduction relue est réintégrée, l'entrée d'origine reste la source de vérité
    /// pour le reste.
    /// </summary>
    /// <param name="entries">Entrées du projet à mettre à jour.</param>
    /// <param name="path">Chemin du fichier CSV relu.</param>
    /// <returns>Le nombre d'entrées effectivement mises à jour.</returns>
    /// <exception cref="InvalidDataException">Le fichier n'a pas l'en-tête CSV attendu.</exception>
    public static int Import(IReadOnlyList<TranslationEntry> entries, string path)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        Dictionary<string, TranslationEntry> byId = new(StringComparer.Ordinal);
        foreach (TranslationEntry entry in entries)
        {
            byId[entry.Id] = entry;
        }

        using StreamReader reader = new(path, Encoding.UTF8);
        List<List<string>> rows = ParseCsv(reader.ReadToEnd());

        if (rows.Count == 0 || !RowMatchesHeader(rows[0]))
        {
            throw new InvalidDataException("Le fichier CSV n'a pas l'en-tête attendu pour un import de traductions RomTranslator.");
        }

        int updatedCount = 0;
        for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            List<string> fields = rows[rowIndex];
            if (fields.Count < 3)
            {
                continue;
            }

            if (byId.TryGetValue(fields[0], out TranslationEntry? entry)
                && !string.Equals(entry.TranslatedText, fields[2], StringComparison.Ordinal))
            {
                entry.TranslatedText = fields[2];
                updatedCount++;
            }
        }

        return updatedCount;
    }

    private static bool RowMatchesHeader(List<string> row)
    {
        if (row.Count != Header.Length)
        {
            return false;
        }

        for (int i = 0; i < Header.Length; i++)
        {
            if (!string.Equals(row[i], Header[i], StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static string Escape(string value)
    {
        bool mustQuote = value.Contains(',', StringComparison.Ordinal)
            || value.Contains('"', StringComparison.Ordinal)
            || value.Contains('\n', StringComparison.Ordinal)
            || value.Contains('\r', StringComparison.Ordinal);

        if (!mustQuote)
        {
            return value;
        }

        return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    /// <summary>
    /// Découpe un document CSV complet en lignes de champs, en respectant les retours à la ligne situés
    /// à l'intérieur d'un champ entre guillemets (RFC 4180). Une ligne vide en dehors de tout champ
    /// entre guillemets termine le document sans produire de ligne supplémentaire.
    /// </summary>
    private static List<List<string>> ParseCsv(string content)
    {
        List<List<string>> rows = new();
        List<string> currentRow = new();
        StringBuilder current = new();
        bool inQuotes = false;
        bool rowHasContent = false;

        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];

            if (inQuotes)
            {
                if (c == '"' && i + 1 < content.Length && content[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    inQuotes = false;
                }
                else
                {
                    current.Append(c);
                }

                continue;
            }

            switch (c)
            {
                case '"':
                    inQuotes = true;
                    rowHasContent = true;
                    break;
                case ',':
                    currentRow.Add(current.ToString());
                    current.Clear();
                    rowHasContent = true;
                    break;
                case '\r':
                    break;
                case '\n':
                    if (rowHasContent || current.Length > 0)
                    {
                        currentRow.Add(current.ToString());
                        rows.Add(currentRow);
                    }

                    currentRow = new List<string>();
                    current.Clear();
                    rowHasContent = false;
                    break;
                default:
                    current.Append(c);
                    rowHasContent = true;
                    break;
            }
        }

        if (rowHasContent || current.Length > 0)
        {
            currentRow.Add(current.ToString());
            rows.Add(currentRow);
        }

        return rows;
    }
}
