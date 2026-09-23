// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.Core.Binary;

/// <summary>Une plage d'octets qui diffère entre deux images, à un même décalage.</summary>
/// <param name="Offset">Décalage de début de la plage, dans les deux images.</param>
/// <param name="OriginalBytes">Octets de l'image originale sur cette plage.</param>
/// <param name="ModifiedBytes">Octets de l'image modifiée sur cette plage.</param>
public sealed record BinaryDifference(long Offset, byte[] OriginalBytes, byte[] ModifiedBytes)
{
    /// <summary>Longueur de la plage qui diffère.</summary>
    public int Length => OriginalBytes.Length;
}
