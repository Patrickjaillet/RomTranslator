// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace RomTranslator.Modules.SegaSaturn.CharacterTables;

/// <summary>Écrit une table de caractères au format <c>.tbl</c> standard (voir <see cref="CharacterTableFileReader" />).</summary>
public static class CharacterTableFileWriter
{
    private static readonly UTF8Encoding _utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>Enregistre une table de caractères dans un fichier <c>.tbl</c>.</summary>
    /// <param name="table">Table à enregistrer.</param>
    /// <param name="path">Chemin du fichier <c>.tbl</c> à créer.</param>
    public static void Write(CharacterTableFile table, string path)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using StreamWriter writer = new(path, append: false, _utf8WithoutBom);

        foreach (CharacterTableEntry entry in table.Entries)
        {
            writer.WriteLine(ToHex(entry.Bytes) + "=" + entry.Text);
        }

        if (table.NewLineBytes is { Count: > 0 })
        {
            writer.WriteLine("*" + ToHex(table.NewLineBytes));
        }

        if (table.EndOfTextBytes is { Count: > 0 })
        {
            writer.WriteLine("\\" + ToHex(table.EndOfTextBytes));
        }
    }

    private static string ToHex(IReadOnlyList<byte> bytes)
    {
        StringBuilder builder = new(bytes.Count * 2);
        foreach (byte value in bytes)
        {
            builder.Append(value.ToString("X2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}
