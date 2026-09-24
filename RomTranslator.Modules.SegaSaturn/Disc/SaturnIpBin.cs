// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.IO;
using System.Text;
using RomTranslator.Core.Localization;

namespace RomTranslator.Modules.SegaSaturn.Disc;

/// <summary>
/// Champs de l'en-tête IP.BIN (Initial Program) lus dans les 256 premiers octets du disque. Structure et
/// décalages vérifiés par extraction hexadécimale sur une image homebrew et un jeu commercial officiel,
/// recoupés avec le gabarit du SDK homebrew Yaul (voir <c>docs/technique/SegaSaturn.md</c> § 3).
/// </summary>
/// <param name="HardwareId">Identifiant matériel, attendu égal à <see cref="SaturnIpBin.ExpectedHardwareId"/>.</param>
/// <param name="MakerId">Identifiant du fabricant/éditeur.</param>
/// <param name="ProductNumber">Numéro de produit.</param>
/// <param name="Version">Version du produit.</param>
/// <param name="ReleaseDate">Date de sortie, telle qu'écrite sur le disque (format attendu AAAAMMJJ).</param>
/// <param name="DeviceInformation">Informations sur le support (par exemple « CD-1/1 »).</param>
/// <param name="AreaSymbols">Symboles de zone/région cible.</param>
/// <param name="Peripherals">Symboles des périphériques compatibles.</param>
/// <param name="GameTitle">Titre du jeu.</param>
public sealed record SaturnIpBinHeader(
    string HardwareId,
    string MakerId,
    string ProductNumber,
    string Version,
    string ReleaseDate,
    string DeviceInformation,
    string AreaSymbols,
    string Peripherals,
    string GameTitle);

/// <summary>Lit l'en-tête IP.BIN situé au tout début de la zone système d'un disque Saturn.</summary>
public static class SaturnIpBin
{
    private static readonly CompositeFormat HeaderTooShortFormat = CompositeFormat.Parse(Strings.Saturn_Error_HeaderTooShort);

    /// <summary>Identifiant matériel attendu en tête d'une image Saturn valide.</summary>
    public const string ExpectedHardwareId = "SEGA SEGASATURN ";

    /// <summary>Taille de la région de l'en-tête utile à RomTranslator (jusqu'au titre du jeu inclus).</summary>
    public const int HeaderRegionSize = 0x0D0;

    private const int HardwareIdOffset = 0x000;
    private const int HardwareIdLength = 16;
    private const int MakerIdOffset = 0x010;
    private const int MakerIdLength = 16;
    private const int ProductNumberOffset = 0x020;
    private const int ProductNumberLength = 10;
    private const int VersionOffset = 0x02A;
    private const int VersionLength = 6;
    private const int ReleaseDateOffset = 0x030;
    private const int ReleaseDateLength = 8;
    private const int DeviceInformationOffset = 0x038;
    private const int DeviceInformationLength = 8;
    private const int AreaSymbolsOffset = 0x040;
    private const int AreaSymbolsLength = 10;
    private const int PeripheralsOffset = 0x050;
    private const int PeripheralsLength = 16;
    private const int GameTitleOffset = 0x060;
    private const int GameTitleLength = 112;

    /// <summary>Analyse les octets de l'en-tête IP.BIN.</summary>
    /// <param name="headerBytes">Au moins les <see cref="HeaderRegionSize" /> premiers octets de la zone système du disque.</param>
    /// <exception cref="InvalidDataException"><paramref name="headerBytes" /> est trop court pour contenir l'en-tête.</exception>
    public static SaturnIpBinHeader Parse(byte[] headerBytes)
    {
        ArgumentNullException.ThrowIfNull(headerBytes);

        if (headerBytes.Length < HeaderRegionSize)
        {
            throw new InvalidDataException(
                string.Format(CultureInfo.CurrentCulture, HeaderTooShortFormat, headerBytes.Length, HeaderRegionSize));
        }

        return new SaturnIpBinHeader(
            HardwareId: ReadAscii(headerBytes, HardwareIdOffset, HardwareIdLength),
            MakerId: ReadAscii(headerBytes, MakerIdOffset, MakerIdLength),
            ProductNumber: ReadAscii(headerBytes, ProductNumberOffset, ProductNumberLength),
            Version: ReadAscii(headerBytes, VersionOffset, VersionLength),
            ReleaseDate: ReadAscii(headerBytes, ReleaseDateOffset, ReleaseDateLength),
            DeviceInformation: ReadAscii(headerBytes, DeviceInformationOffset, DeviceInformationLength),
            AreaSymbols: ReadAscii(headerBytes, AreaSymbolsOffset, AreaSymbolsLength),
            Peripherals: ReadAscii(headerBytes, PeripheralsOffset, PeripheralsLength),
            GameTitle: ReadAscii(headerBytes, GameTitleOffset, GameTitleLength));
    }

    private static string ReadAscii(byte[] data, int offset, int length)
    {
        return Encoding.ASCII.GetString(data, offset, length).TrimEnd(' ');
    }
}
