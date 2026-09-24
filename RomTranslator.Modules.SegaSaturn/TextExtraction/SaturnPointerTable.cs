// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;

namespace RomTranslator.Modules.SegaSaturn.TextExtraction;

/// <summary>Un pointeur trouvé dans la ROM : son propre emplacement, et l'offset de chaîne qu'il désigne.</summary>
/// <param name="PointerOffset">Décalage, dans la ROM, de la valeur du pointeur elle-même.</param>
/// <param name="TargetOffset">Offset de la chaîne de texte désignée par ce pointeur.</param>
public sealed record PointerLocation(long PointerOffset, long TargetOffset);

/// <summary>
/// Lit et écrit une table de pointeurs vers des chaînes de texte, selon un <see cref="PointerEncoding" />
/// propre au jeu ciblé. Une table de pointeurs est une plage contiguë de la ROM contenant une suite de
/// valeurs de même taille, chacune désignant une chaîne de texte.
/// </summary>
public sealed class SaturnPointerTable
{
    private readonly PointerEncoding _encoding;

    /// <summary>Initialise la table de pointeurs avec l'encodage du jeu ciblé.</summary>
    public SaturnPointerTable(PointerEncoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);

        _encoding = encoding;
    }

    /// <summary>
    /// Lit une plage contiguë de pointeurs à partir des octets fournis (qui doivent commencer exactement à
    /// <paramref name="pointerTableOffset" /> dans la ROM).
    /// </summary>
    /// <param name="romBytes">Octets de la plage lue depuis la ROM, à partir de <paramref name="pointerTableOffset" />.</param>
    /// <param name="pointerTableOffset">Décalage, dans la ROM, du premier octet de <paramref name="romBytes" />.</param>
    /// <param name="pointerCount">Nombre de pointeurs à lire.</param>
    /// <exception cref="ArgumentException"><paramref name="romBytes" /> est trop court pour contenir <paramref name="pointerCount" /> pointeurs.</exception>
    public IReadOnlyList<PointerLocation> ReadPointers(IReadOnlyList<byte> romBytes, long pointerTableOffset, int pointerCount)
    {
        ArgumentNullException.ThrowIfNull(romBytes);

        int requiredLength = pointerCount * _encoding.ByteWidth;
        if (romBytes.Count < requiredLength)
        {
            throw new ArgumentException(
                $"Les données fournies ({romBytes.Count} octets) sont trop courtes pour {pointerCount} pointeurs de {_encoding.ByteWidth} octets.",
                nameof(romBytes));
        }

        List<PointerLocation> pointers = new(pointerCount);
        for (int i = 0; i < pointerCount; i++)
        {
            int startIndex = i * _encoding.ByteWidth;
            long rawValue = ReadValue(romBytes, startIndex);
            long pointerOffset = pointerTableOffset + startIndex;
            long targetOffset = _encoding.ResolveTargetOffset(rawValue);

            pointers.Add(new PointerLocation(pointerOffset, targetOffset));
        }

        return pointers;
    }

    /// <summary>Encode un pointeur vers un offset de chaîne, prêt à être écrit dans la ROM.</summary>
    /// <param name="targetOffset">Offset réel de la chaîne visée.</param>
    public IReadOnlyList<byte> EncodePointer(long targetOffset)
    {
        long rawValue = _encoding.ComputeRawValue(targetOffset);
        byte[] bytes = new byte[_encoding.ByteWidth];

        for (int i = 0; i < _encoding.ByteWidth; i++)
        {
            int shift = _encoding.IsBigEndian ? (_encoding.ByteWidth - 1 - i) * 8 : i * 8;
            bytes[i] = (byte)((rawValue >> shift) & 0xFF);
        }

        return bytes;
    }

    private long ReadValue(IReadOnlyList<byte> bytes, int startIndex)
    {
        long value = 0;
        for (int i = 0; i < _encoding.ByteWidth; i++)
        {
            byte b = bytes[startIndex + i];
            int shift = _encoding.IsBigEndian ? (_encoding.ByteWidth - 1 - i) * 8 : i * 8;
            value |= (long)b << shift;
        }

        return value;
    }
}
