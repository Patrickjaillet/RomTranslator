// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RomTranslator.Core.Localization;

namespace RomTranslator.Modules.SegaSaturn.Disc;

/// <summary>
/// Analyseur minimal du système de fichiers ISO9660 utilisé par les disques Saturn (avec extensions
/// propres à Saturn non interprétées ici : seuls les champs standards nécessaires à la résolution de
/// fichiers sont lus). Structure vérifiée par extraction hexadécimale sur une image homebrew réelle
/// (voir <c>docs/technique/SegaSaturn.md</c> § 2) : descripteur de volume primaire au secteur 16, registres
/// de répertoire ISO9660 standards (identifiants little-endian/big-endian dupliqués).
/// </summary>
public sealed class Iso9660Reader
{
    private const long PrimaryVolumeDescriptorSector = 16;
    private const int RootDirectoryRecordOffset = 156;

    private readonly SectorReader _sectorReader;

    /// <summary>Ouvre le système de fichiers d'une piste de données.</summary>
    /// <param name="sectorReader">Lecteur de secteurs de la piste de données du disque.</param>
    public Iso9660Reader(SectorReader sectorReader)
    {
        ArgumentNullException.ThrowIfNull(sectorReader);

        _sectorReader = sectorReader;

        byte[] volumeDescriptor = _sectorReader.ReadSector(PrimaryVolumeDescriptorSector);
        if (volumeDescriptor[0] != 1 || Encoding.ASCII.GetString(volumeDescriptor, 1, 5) != "CD001")
        {
            throw new InvalidDataException(Strings.Saturn_Error_VolumeDescriptorNotFound);
        }

        Root = ReadDirectoryRecord(volumeDescriptor, RootDirectoryRecordOffset);
    }

    /// <summary>Entrée racine du système de fichiers.</summary>
    public Iso9660Entry Root { get; }

    /// <summary>Liste le contenu direct (non récursif) d'un dossier.</summary>
    /// <param name="directory">Dossier à lister (voir <see cref="Root" /> pour la racine).</param>
    public IReadOnlyList<Iso9660Entry> ListDirectory(Iso9660Entry directory)
    {
        ArgumentNullException.ThrowIfNull(directory);
        if (!directory.IsDirectory)
        {
            throw new ArgumentException("L'entrée fournie n'est pas un dossier.", nameof(directory));
        }

        byte[] data = _sectorReader.ReadBytes(directory.StartSector, (int)directory.LengthInBytes);
        List<Iso9660Entry> entries = new();

        int position = 0;
        while (position < data.Length)
        {
            int recordLength = data[position];
            if (recordLength == 0)
            {
                // Un enregistrement de longueur nulle marque la fin des entrées du secteur courant ;
                // les enregistrements ne franchissent jamais une frontière de secteur en ISO9660.
                position = AlignToNextSector(position);
                continue;
            }

            // Les pseudo-entrées "." (dossier courant, nom = octet 0x00) et ".." (dossier parent,
            // nom = octet 0x01) ne sont pas exposées : RomTranslator navigue par nom de fichier explicite.
            int nameLength = data[position + 32];
            bool isSelfOrParent = nameLength == 1 && data[position + 33] is 0x00 or 0x01;

            if (!isSelfOrParent)
            {
                entries.Add(ReadDirectoryRecord(data, position));
            }

            position += recordLength;
        }

        return entries;
    }

    /// <summary>
    /// Résout un chemin de fichier relatif à la racine (segments séparés par <c>/</c>) vers son entrée.
    /// </summary>
    /// <param name="path">Chemin relatif à la racine, par exemple <c>"0.BIN"</c> ou <c>"DATA/SCRIPT.BIN"</c>.</param>
    /// <returns>L'entrée correspondante, ou <see langword="null" /> si le chemin n'existe pas.</returns>
    public Iso9660Entry? FindEntry(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        Iso9660Entry current = Root;
        foreach (string segment in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            IReadOnlyList<Iso9660Entry> siblings = ListDirectory(current);
            Iso9660Entry? match = siblings.FirstOrDefault(entry => string.Equals(entry.Name, segment, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                return null;
            }

            current = match;
        }

        return current;
    }

    private static int AlignToNextSector(int position)
    {
        int sectorStart = (position / SectorReader.SectorDataSize) * SectorReader.SectorDataSize;
        return sectorStart + SectorReader.SectorDataSize;
    }

    private static Iso9660Entry ReadDirectoryRecord(byte[] data, int offset)
    {
        long startSector = ReadUInt32LittleEndian(data, offset + 2);
        long length = ReadUInt32LittleEndian(data, offset + 10);
        byte flags = data[offset + 25];
        bool isDirectory = (flags & 0x02) != 0;

        int nameLength = data[offset + 32];
        string rawName = Encoding.ASCII.GetString(data, offset + 33, nameLength);

        string name = isDirectory ? rawName : StripVersionSuffix(rawName);

        return new Iso9660Entry(name, isDirectory, startSector, length);
    }

    private static string StripVersionSuffix(string rawName)
    {
        int separator = rawName.IndexOf(';', StringComparison.Ordinal);
        return separator >= 0 ? rawName[..separator] : rawName;
    }

    private static uint ReadUInt32LittleEndian(byte[] data, int offset)
    {
        return (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24));
    }
}
