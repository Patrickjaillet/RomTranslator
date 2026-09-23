// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.IO;

namespace RomTranslator.Core.Binary;

/// <summary>
/// Lit un fichier binaire (image ROM) par lignes de 16 octets, pour un visualiseur hexadécimal en lecture
/// seule. Ne charge jamais le fichier entier en mémoire : chaque appel lit uniquement la plage demandée.
/// </summary>
public sealed class HexDumpReader
{
    /// <summary>Nombre d'octets par ligne.</summary>
    public const int BytesPerLine = 16;

    private readonly string _path;

    /// <summary>Initialise le lecteur pour un fichier.</summary>
    /// <param name="path">Chemin du fichier à visualiser.</param>
    public HexDumpReader(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _path = path;
        Length = new FileInfo(path).Length;
    }

    /// <summary>Taille totale du fichier, en octets.</summary>
    public long Length { get; }

    /// <summary>Nombre total de lignes de 16 octets (la dernière peut être incomplète).</summary>
    public long LineCount => Length == 0 ? 0 : (Length + BytesPerLine - 1) / BytesPerLine;

    /// <summary>
    /// Lit une plage de lignes, à partir de la ligne d'index <paramref name="startLine" /> (0-based).
    /// </summary>
    /// <param name="startLine">Index de la première ligne à lire.</param>
    /// <param name="lineCount">Nombre de lignes à lire au maximum (moins si la fin du fichier est atteinte).</param>
    public IReadOnlyList<HexDumpLine> ReadLines(long startLine, int lineCount)
    {
        if (startLine < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startLine), startLine, "L'index de ligne ne peut pas être négatif.");
        }

        if (lineCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lineCount), lineCount, "Le nombre de lignes ne peut pas être négatif.");
        }

        List<HexDumpLine> lines = new();
        if (lineCount == 0 || startLine >= LineCount)
        {
            return lines;
        }

        long startOffset = startLine * BytesPerLine;
        int bytesToRead = (int)Math.Min((long)lineCount * BytesPerLine, Length - startOffset);
        byte[] buffer = new byte[bytesToRead];

        using (FileStream stream = File.OpenRead(_path))
        {
            stream.Seek(startOffset, SeekOrigin.Begin);
            int totalRead = 0;
            while (totalRead < bytesToRead)
            {
                int read = stream.Read(buffer, totalRead, bytesToRead - totalRead);
                if (read == 0)
                {
                    break;
                }

                totalRead += read;
            }
        }

        for (int position = 0; position < buffer.Length; position += BytesPerLine)
        {
            int length = Math.Min(BytesPerLine, buffer.Length - position);
            byte[] lineBytes = new byte[length];
            Array.Copy(buffer, position, lineBytes, 0, length);
            lines.Add(new HexDumpLine(startOffset + position, lineBytes));
        }

        return lines;
    }
}
