// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

namespace RomTranslator.Modules.SegaSaturn.Disc;

/// <summary>Une entrée (fichier ou dossier) du système de fichiers ISO9660 d'un disque Saturn.</summary>
/// <param name="Name">
/// Nom de l'entrée, tel qu'enregistré sur le disque (le suffixe de version ISO9660, par exemple <c>;1</c>,
/// est retiré des noms de fichiers).
/// </param>
/// <param name="IsDirectory">Indique si l'entrée est un dossier.</param>
/// <param name="StartSector">Secteur de début des données de l'entrée.</param>
/// <param name="LengthInBytes">Taille de l'entrée, en octets.</param>
public sealed record Iso9660Entry(string Name, bool IsDirectory, long StartSector, long LengthInBytes);
