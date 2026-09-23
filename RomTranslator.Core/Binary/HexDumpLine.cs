// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.Core.Binary;

/// <summary>Une ligne du visualiseur hexadécimal : décalage, octets et leur représentation imprimable.</summary>
/// <param name="Offset">Décalage du premier octet de la ligne.</param>
/// <param name="Bytes">Octets de la ligne (16 au maximum, moins pour la dernière ligne d'un fichier).</param>
public sealed record HexDumpLine(long Offset, byte[] Bytes)
{
    /// <summary>Décalage affiché en hexadécimal sur 8 chiffres (par exemple <c>"000001A0"</c>).</summary>
    public string OffsetHex => Offset.ToString("X8", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>Octets affichés en hexadécimal, séparés par un espace (par exemple <c>"4E 45 53 1A"</c>).</summary>
    public string HexColumn => string.Join(' ', System.Array.ConvertAll(Bytes, b => b.ToString("X2", System.Globalization.CultureInfo.InvariantCulture)));

    /// <summary>
    /// Représentation imprimable des octets : les caractères ASCII imprimables (0x20 à 0x7E) tels quels,
    /// un point (<c>.</c>) pour tout autre octet.
    /// </summary>
    public string AsciiColumn
    {
        get
        {
            char[] characters = new char[Bytes.Length];
            for (int i = 0; i < Bytes.Length; i++)
            {
                characters[i] = Bytes[i] is >= 0x20 and <= 0x7E ? (char)Bytes[i] : '.';
            }

            return new string(characters);
        }
    }
}
