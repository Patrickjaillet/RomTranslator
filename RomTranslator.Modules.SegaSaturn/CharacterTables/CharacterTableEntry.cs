// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;

namespace RomTranslator.Modules.SegaSaturn.CharacterTables;

/// <summary>Une correspondance entre une séquence d'octets et le texte qu'elle représente.</summary>
/// <param name="Bytes">Séquence d'octets (un octet pour une entrée simple, plusieurs pour du DTE/MTE).</param>
/// <param name="Text">Texte représenté par cette séquence (un ou plusieurs caractères).</param>
public sealed record CharacterTableEntry(IReadOnlyList<byte> Bytes, string Text);
