// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using RomTranslator.Core.Localization;

namespace RomTranslator.Modules.SegaSaturn.CharacterTables;

/// <summary>
/// Lit un fichier de table de caractères au format <c>.tbl</c> standard de la communauté romhacking :
/// une entrée par ligne, <c>HEX=texte</c> (correspondance), <c>*HEX</c> (saut de ligne) ou <c>\HEX</c>
/// (fin de texte) ; les lignes vides et celles commençant par <c>#</c> sont ignorées.
/// </summary>
public static class CharacterTableFileReader
{
    /// <summary>Lit et analyse un fichier <c>.tbl</c>.</summary>
    /// <param name="path">Chemin du fichier <c>.tbl</c>, encodé en UTF-8.</param>
    /// <exception cref="InvalidDataException">Une ligne du fichier n'est pas syntaxiquement valide.</exception>
    public static CharacterTableFile Read(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return Parse(File.ReadLines(path, Encoding.UTF8));
    }

    /// <summary>Analyse le contenu d'une table de caractères déjà chargée en mémoire, ligne par ligne.</summary>
    /// <param name="lines">Lignes du fichier <c>.tbl</c>.</param>
    public static CharacterTableFile Parse(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        List<CharacterTableEntry> entries = new();
        IReadOnlyList<byte>? newLineBytes = null;
        IReadOnlyList<byte>? endOfTextBytes = null;

        foreach (string rawLine in lines)
        {
            string line = rawLine.TrimEnd('\r');
            if (line.Length == 0 || line[0] == '#')
            {
                continue;
            }

            if (line[0] == '*')
            {
                newLineBytes = ParseHex(line[1..], rawLine);
                continue;
            }

            if (line[0] == '\\')
            {
                int separatorIndex = line.IndexOf('=', StringComparison.Ordinal);
                string hexPart = separatorIndex >= 0 ? line[1..separatorIndex] : line[1..];
                endOfTextBytes = ParseHex(hexPart, rawLine);
                continue;
            }

            int equalsIndex = line.IndexOf('=', StringComparison.Ordinal);
            if (equalsIndex < 0)
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.Saturn_Error_TableMalformedLine, rawLine));
            }

            IReadOnlyList<byte> bytes = ParseHex(line[..equalsIndex], rawLine);
            string text = line[(equalsIndex + 1)..];
            entries.Add(new CharacterTableEntry(bytes, text));
        }

        return new CharacterTableFile(entries, newLineBytes, endOfTextBytes);
    }

    private static IReadOnlyList<byte> ParseHex(string hex, string rawLine)
    {
        if (hex.Length == 0 || hex.Length % 2 != 0)
        {
            throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.Saturn_Error_TableInvalidHex, rawLine));
        }

        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            if (!byte.TryParse(hex.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i]))
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.Saturn_Error_TableInvalidHex, rawLine));
            }
        }

        return bytes;
    }
}
