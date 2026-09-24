// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;
using System.Text;

namespace RomTranslator.Tests.SegaSaturn.Support;

/// <summary>
/// Construit une image disque Saturn minimale et entièrement synthétique (BIN/CUE, secteurs
/// <c>MODE1/2048</c>) pour les tests : en-tête IP.BIN, descripteur de volume primaire ISO9660 et un
/// répertoire racine avec un unique fichier de données. Ne contient aucun contenu de jeu réel.
/// </summary>
internal static class SyntheticSaturnDiscBuilder
{
    private const int SectorSize = 2048;

    /// <summary>Construit une image <c>.cue</c>/<c>.iso</c> synthétique dans le dossier indiqué.</summary>
    /// <param name="directory">Dossier de destination.</param>
    /// <param name="gameTitle">Titre du jeu écrit dans l'IP.BIN.</param>
    /// <param name="makerId">Identifiant fabricant écrit dans l'IP.BIN.</param>
    /// <param name="productNumber">Numéro de produit écrit dans l'IP.BIN.</param>
    /// <param name="areaSymbols">Symboles de zone écrits dans l'IP.BIN.</param>
    /// <param name="validHardwareId">
    /// Si <see langword="false" />, remplace l'identifiant matériel par une chaîne invalide (pour tester le rejet).
    /// </param>
    public static string Build(
        string directory,
        string gameTitle = "TEST GAME",
        string makerId = "SEGA ENTERPRISES",
        string productNumber = "T-000000  ",
        string areaSymbols = "JTUE      ",
        bool validHardwareId = true)
    {
        byte[] ipBinSector = BuildIpBinSector(gameTitle, makerId, productNumber, areaSymbols, validHardwareId);
        byte[] primaryVolumeDescriptor = BuildPrimaryVolumeDescriptor(rootDirectorySector: 18, rootDirectoryLength: SectorSize);
        byte[] rootDirectorySector = BuildRootDirectorySector(fileSector: 19, fileLength: SectorSize, fileName: "0.BIN");
        byte[] fileSector = BuildFilledSector(0xAB);

        using MemoryStream image = new();
        // Secteur 0 : IP.BIN. Secteurs 1-15 : remplissage de la zone système. Secteur 16 : descripteur
        // de volume primaire ISO9660. Secteur 17 : remplissage. Secteur 18 : répertoire racine. Secteur 19 : fichier.
        WriteSector(image, ipBinSector);
        for (int sector = 1; sector <= 15; sector++)
        {
            WriteSector(image, BuildFilledSector(0x00));
        }

        WriteSector(image, primaryVolumeDescriptor);
        WriteSector(image, BuildFilledSector(0x00));
        WriteSector(image, rootDirectorySector);
        WriteSector(image, fileSector);

        string isoPath = Path.Combine(directory, "test.iso");
        File.WriteAllBytes(isoPath, image.ToArray());

        string cuePath = Path.Combine(directory, "test.cue");
        File.WriteAllText(cuePath, "FILE \"test.iso\" BINARY\n  TRACK 01 MODE1/2048\n    INDEX 01 00:00:00\n");

        return cuePath;
    }

    private static void WriteSector(Stream stream, byte[] sector)
    {
        if (sector.Length != SectorSize)
        {
            throw new InvalidOperationException("Un secteur synthétique doit faire exactement 2048 octets.");
        }

        stream.Write(sector);
    }

    private static byte[] BuildFilledSector(byte value)
    {
        byte[] sector = new byte[SectorSize];
        Array.Fill(sector, value);
        return sector;
    }

    private static byte[] BuildIpBinSector(string gameTitle, string makerId, string productNumber, string areaSymbols, bool validHardwareId)
    {
        byte[] sector = new byte[SectorSize];
        Array.Fill(sector, (byte)' ');

        WriteAscii(sector, 0x000, validHardwareId ? "SEGA SEGASATURN " : "NOT A SATURN ROM", 16);
        WriteAscii(sector, 0x010, makerId, 16);
        WriteAscii(sector, 0x020, productNumber, 10);
        WriteAscii(sector, 0x02A, "V1.000", 6);
        WriteAscii(sector, 0x030, "20260101", 8);
        WriteAscii(sector, 0x038, "CD-1/1  ", 8);
        WriteAscii(sector, 0x040, areaSymbols, 10);
        WriteAscii(sector, 0x050, "J               ", 16);
        WriteAscii(sector, 0x060, gameTitle, 112);

        return sector;
    }

    private static byte[] BuildPrimaryVolumeDescriptor(int rootDirectorySector, int rootDirectoryLength)
    {
        byte[] sector = new byte[SectorSize];
        Array.Fill(sector, (byte)' ');

        sector[0] = 1;
        WriteAscii(sector, 1, "CD001", 5);
        sector[6] = 1;

        WriteDirectoryRecord(sector, 156, rootDirectorySector, rootDirectoryLength, isDirectory: true, name: "\0");

        return sector;
    }

    private static byte[] BuildRootDirectorySector(int fileSector, int fileLength, string fileName)
    {
        byte[] sector = new byte[SectorSize];

        int offset = 0;
        offset += WriteDirectoryRecord(sector, offset, 18, SectorSize, isDirectory: true, name: "\0");
        offset += WriteDirectoryRecord(sector, offset, 18, SectorSize, isDirectory: true, name: "\u0001");
        WriteDirectoryRecord(sector, offset, fileSector, fileLength, isDirectory: false, name: fileName + ";1");

        return sector;
    }

    private static int WriteDirectoryRecord(byte[] buffer, int offset, int startSector, int length, bool isDirectory, string name)
    {
        byte[] nameBytes = Encoding.ASCII.GetBytes(name);
        int nameFieldLength = nameBytes.Length;
        int recordLength = 33 + nameFieldLength + (nameFieldLength % 2 == 0 ? 1 : 0);

        buffer[offset] = (byte)recordLength;
        buffer[offset + 1] = 0;

        WriteUInt32BothEndian(buffer, offset + 2, startSector);
        WriteUInt32BothEndian(buffer, offset + 10, length);

        buffer[offset + 25] = (byte)(isDirectory ? 0x02 : 0x00);
        buffer[offset + 26] = 0;
        buffer[offset + 27] = 0;

        buffer[offset + 32] = (byte)nameFieldLength;
        Array.Copy(nameBytes, 0, buffer, offset + 33, nameFieldLength);

        return recordLength;
    }

    private static void WriteUInt32BothEndian(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value & 0xFF);
        buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
        buffer[offset + 3] = (byte)((value >> 24) & 0xFF);

        buffer[offset + 4] = (byte)((value >> 24) & 0xFF);
        buffer[offset + 5] = (byte)((value >> 16) & 0xFF);
        buffer[offset + 6] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 7] = (byte)(value & 0xFF);
    }

    private static void WriteAscii(byte[] buffer, int offset, string value, int length)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(value.PadRight(length).Substring(0, length));
        Array.Copy(bytes, 0, buffer, offset, length);
    }
}
