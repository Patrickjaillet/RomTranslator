// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.IO;

namespace RomTranslator.Core.Binary;

/// <summary>
/// Compare deux fichiers binaires octet par octet (ROM originale et ROM modifiée) et regroupe les octets
/// contigus qui diffèrent en plages. Une différence de longueur entre les deux fichiers est traitée comme
/// une plage supplémentaire à la fin du plus court, comparée à des octets absents (valeur 0).
/// </summary>
public static class BinaryComparer
{
    private const int BufferSize = 64 * 1024;

    /// <summary>Compare deux fichiers et retourne leurs plages d'octets qui diffèrent, dans l'ordre des décalages.</summary>
    /// <param name="originalPath">Chemin du fichier original.</param>
    /// <param name="modifiedPath">Chemin du fichier modifié.</param>
    public static IReadOnlyList<BinaryDifference> Compare(string originalPath, string modifiedPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originalPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(modifiedPath);

        using FileStream original = File.OpenRead(originalPath);
        using FileStream modified = File.OpenRead(modifiedPath);

        List<BinaryDifference> differences = new();
        List<byte> pendingOriginal = new();
        List<byte> pendingModified = new();
        long pendingStart = -1;

        byte[] originalBuffer = new byte[BufferSize];
        byte[] modifiedBuffer = new byte[BufferSize];
        long offset = 0;

        while (true)
        {
            int originalRead = ReadFully(original, originalBuffer);
            int modifiedRead = ReadFully(modified, modifiedBuffer);
            int readCount = Math.Max(originalRead, modifiedRead);

            if (readCount == 0)
            {
                break;
            }

            for (int i = 0; i < readCount; i++)
            {
                byte originalByte = i < originalRead ? originalBuffer[i] : (byte)0;
                byte modifiedByte = i < modifiedRead ? modifiedBuffer[i] : (byte)0;

                if (originalByte != modifiedByte)
                {
                    if (pendingStart < 0)
                    {
                        pendingStart = offset + i;
                    }

                    pendingOriginal.Add(originalByte);
                    pendingModified.Add(modifiedByte);
                }
                else if (pendingStart >= 0)
                {
                    differences.Add(new BinaryDifference(pendingStart, pendingOriginal.ToArray(), pendingModified.ToArray()));
                    pendingOriginal.Clear();
                    pendingModified.Clear();
                    pendingStart = -1;
                }
            }

            offset += readCount;

            if (originalRead < BufferSize && modifiedRead < BufferSize)
            {
                break;
            }
        }

        if (pendingStart >= 0)
        {
            differences.Add(new BinaryDifference(pendingStart, pendingOriginal.ToArray(), pendingModified.ToArray()));
        }

        return differences;
    }

    private static int ReadFully(FileStream stream, byte[] buffer)
    {
        int totalRead = 0;

        while (totalRead < buffer.Length)
        {
            int read = stream.Read(buffer, totalRead, buffer.Length - totalRead);
            if (read == 0)
            {
                break;
            }

            totalRead += read;
        }

        return totalRead;
    }
}
