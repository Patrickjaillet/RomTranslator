// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.IO;

namespace RomTranslator.Core.Binary;

/// <summary>
/// Génère et applique des patchs au format IPS (International Patching System). Le format encode des
/// enregistrements <c>décalage (3 octets) + taille (2 octets) + données</c> ; le décalage étant limité à
/// 3 octets, ce format ne peut pas cibler un octet au-delà de 16 Mio (0xFFFFFF), une limite compatible avec
/// la plupart des ROMs de consoles 16/32 bits.
/// </summary>
public static class IpsPatch
{
    /// <summary>Décalage maximal représentable par un patch IPS (3 octets).</summary>
    public const long MaxOffset = 0xFFFFFF;

    private static readonly byte[] _header = { (byte)'P', (byte)'A', (byte)'T', (byte)'C', (byte)'H' };
    private static readonly byte[] _footer = { (byte)'E', (byte)'O', (byte)'F' };

    /// <summary>
    /// Génère un patch IPS à partir des différences entre une image originale et une image modifiée, et
    /// l'enregistre dans <paramref name="patchPath" />.
    /// </summary>
    /// <param name="originalPath">Chemin de l'image ROM d'origine.</param>
    /// <param name="modifiedPath">Chemin de l'image ROM modifiée.</param>
    /// <param name="patchPath">Chemin du fichier patch <c>.ips</c> à créer.</param>
    /// <exception cref="NotSupportedException">
    /// Une différence se situe au-delà du décalage maximal représentable par le format IPS (16 Mio).
    /// </exception>
    public static void Create(string originalPath, string modifiedPath, string patchPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(patchPath);

        IReadOnlyList<BinaryDifference> differences = BinaryComparer.Compare(originalPath, modifiedPath);

        using FileStream output = File.Create(patchPath);
        output.Write(_header);

        foreach (BinaryDifference difference in differences)
        {
            if (difference.Offset > MaxOffset || difference.Offset + difference.Length - 1 > MaxOffset)
            {
                throw new NotSupportedException(
                    $"Le format IPS ne peut pas représenter une différence au décalage 0x{difference.Offset:X} : "
                    + $"la limite du format est 0x{MaxOffset:X} (16 Mio).");
            }

            WriteRecord(output, difference.Offset, difference.ModifiedBytes);
        }

        output.Write(_footer);
    }

    /// <summary>Applique un patch IPS à une image ROM et enregistre le résultat dans un nouveau fichier.</summary>
    /// <param name="sourcePath">Chemin de l'image ROM à patcher (jamais modifiée).</param>
    /// <param name="patchPath">Chemin du fichier patch <c>.ips</c>.</param>
    /// <param name="outputPath">Chemin de l'image ROM patchée à créer.</param>
    /// <exception cref="InvalidDataException">Le fichier n'est pas un patch IPS valide.</exception>
    public static void Apply(string sourcePath, string patchPath, string outputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(patchPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        byte[] source = File.ReadAllBytes(sourcePath);
        byte[] patch = File.ReadAllBytes(patchPath);

        if (patch.Length < _header.Length || !SpanStartsWith(patch, _header))
        {
            throw new InvalidDataException("Le fichier n'est pas un patch IPS valide (en-tête « PATCH » absent).");
        }

        List<byte> result = new(source);
        int position = _header.Length;

        while (true)
        {
            if (position + _footer.Length <= patch.Length && SpanEquals(patch, position, _footer))
            {
                break;
            }

            if (position + 5 > patch.Length)
            {
                throw new InvalidDataException("Le fichier patch IPS est tronqué : enregistrement incomplet.");
            }

            long offset = ReadUInt24BigEndian(patch, position);
            position += 3;
            int size = ReadUInt16BigEndian(patch, position);
            position += 2;

            if (size == 0)
            {
                if (position + 3 > patch.Length)
                {
                    throw new InvalidDataException("Le fichier patch IPS est tronqué : enregistrement RLE incomplet.");
                }

                int rleSize = ReadUInt16BigEndian(patch, position);
                position += 2;
                byte value = patch[position];
                position += 1;

                EnsureCapacity(result, offset + rleSize);
                for (int i = 0; i < rleSize; i++)
                {
                    result[(int)(offset + i)] = value;
                }
            }
            else
            {
                if (position + size > patch.Length)
                {
                    throw new InvalidDataException("Le fichier patch IPS est tronqué : données d'enregistrement incomplètes.");
                }

                EnsureCapacity(result, offset + size);
                for (int i = 0; i < size; i++)
                {
                    result[(int)(offset + i)] = patch[position + i];
                }

                position += size;
            }
        }

        File.WriteAllBytes(outputPath, result.ToArray());
    }

    private static void WriteRecord(Stream output, long offset, byte[] data)
    {
        int remaining = data.Length;
        int dataIndex = 0;

        // Une seule différence peut dépasser 0xFFFF octets (limite de taille d'un enregistrement IPS) :
        // elle est alors découpée en plusieurs enregistrements consécutifs.
        while (remaining > 0)
        {
            int chunkSize = Math.Min(remaining, 0xFFFF);

            WriteUInt24BigEndian(output, offset + dataIndex);
            WriteUInt16BigEndian(output, (ushort)chunkSize);
            output.Write(data, dataIndex, chunkSize);

            dataIndex += chunkSize;
            remaining -= chunkSize;
        }
    }

    private static void EnsureCapacity(List<byte> buffer, long requiredLength)
    {
        while (buffer.Count < requiredLength)
        {
            buffer.Add(0);
        }
    }

    private static bool SpanStartsWith(byte[] data, byte[] prefix) => SpanEquals(data, 0, prefix);

    private static bool SpanEquals(byte[] data, int position, byte[] pattern)
    {
        for (int i = 0; i < pattern.Length; i++)
        {
            if (data[position + i] != pattern[i])
            {
                return false;
            }
        }

        return true;
    }

    private static long ReadUInt24BigEndian(byte[] data, int position)
    {
        return ((long)data[position] << 16) | ((long)data[position + 1] << 8) | data[position + 2];
    }

    private static int ReadUInt16BigEndian(byte[] data, int position)
    {
        return (data[position] << 8) | data[position + 1];
    }

    private static void WriteUInt24BigEndian(Stream output, long value)
    {
        output.WriteByte((byte)((value >> 16) & 0xFF));
        output.WriteByte((byte)((value >> 8) & 0xFF));
        output.WriteByte((byte)(value & 0xFF));
    }

    private static void WriteUInt16BigEndian(Stream output, ushort value)
    {
        output.WriteByte((byte)((value >> 8) & 0xFF));
        output.WriteByte((byte)(value & 0xFF));
    }
}
