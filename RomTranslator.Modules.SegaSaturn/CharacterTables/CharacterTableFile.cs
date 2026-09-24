// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;

namespace RomTranslator.Modules.SegaSaturn.CharacterTables;

/// <summary>
/// Contenu d'un fichier de table de caractères au format <c>.tbl</c> (format standard de la communauté
/// romhacking : lignes <c>HEX=texte</c>, commentaires <c>#</c>, marqueur de saut de ligne <c>*HEX</c>,
/// marqueur de fin de texte <c>\HEX</c>).
/// </summary>
/// <param name="Entries">Correspondances octet(s) ↔ texte.</param>
/// <param name="NewLineBytes">Séquence d'octets représentant un saut de ligne, ou <see langword="null" /> si non définie.</param>
/// <param name="EndOfTextBytes">Séquence d'octets marquant la fin d'une chaîne de texte, ou <see langword="null" /> si non définie.</param>
public sealed record CharacterTableFile(
    IReadOnlyList<CharacterTableEntry> Entries,
    IReadOnlyList<byte>? NewLineBytes,
    IReadOnlyList<byte>? EndOfTextBytes);
