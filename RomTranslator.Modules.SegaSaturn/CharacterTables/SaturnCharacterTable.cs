// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Localization;

namespace RomTranslator.Modules.SegaSaturn.CharacterTables;

/// <summary>
/// Table de correspondance octet(s) ↔ caractère(s) pour un jeu Sega Saturn, construite à partir d'un
/// <see cref="CharacterTableFile" /> (format <c>.tbl</c>). Le décodage essaie les séquences d'octets les
/// plus longues en premier, ce qui permet aux schémas DTE/MTE (une séquence de plusieurs octets représentant
/// plusieurs caractères) de coexister avec des correspondances à un seul octet dans la même table.
/// </summary>
public sealed class SaturnCharacterTable : ICharacterTable
{
    private readonly Dictionary<string, string> _decodeMap;
    private readonly Dictionary<string, byte[]> _encodeMap;
    private readonly int _maxSequenceLength;
    private readonly IReadOnlyList<CharacterTableEntry> _entries;

    /// <summary>Construit la table à partir de son contenu chargé.</summary>
    /// <param name="name">Nom de la table, affiché dans l'interface.</param>
    /// <param name="file">Contenu de la table, tel que lu par <see cref="CharacterTableFileReader" />.</param>
    public SaturnCharacterTable(string name, CharacterTableFile file)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(file);

        Name = name;
        TableFile = file;
        _entries = file.Entries;

        _decodeMap = new Dictionary<string, string>(StringComparer.Ordinal);
        _encodeMap = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        _maxSequenceLength = 1;

        foreach (CharacterTableEntry entry in file.Entries)
        {
            string byteKey = ToKey(entry.Bytes);
            _decodeMap.TryAdd(byteKey, entry.Text);
            _encodeMap.TryAdd(entry.Text, entry.Bytes.ToArray());
            _maxSequenceLength = Math.Max(_maxSequenceLength, entry.Bytes.Count);
        }
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>Contenu de la table tel que chargé, réutilisable pour la réenregistrer ou l'éditer.</summary>
    public CharacterTableFile TableFile { get; }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    /// Un octet de la séquence ne correspond à aucune entrée de la table, même à lui seul.
    /// </exception>
    public string Decode(IReadOnlyList<byte> bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        StringBuilder result = new();
        int position = 0;

        while (position < bytes.Count)
        {
            (string text, int consumed) = DecodeNext(bytes, position);
            result.Append(text);
            position += consumed;
        }

        return result.ToString();
    }

    /// <inheritdoc />
    public IReadOnlyList<byte> Encode(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        List<byte> result = new();
        int position = 0;

        while (position < text.Length)
        {
            (byte[] bytes, int consumed) = EncodeNext(text, position);
            result.AddRange(bytes);
            position += consumed;
        }

        return result;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> Validate()
    {
        List<string> issues = new();
        Dictionary<string, string> seenByteSequences = new(StringComparer.Ordinal);
        Dictionary<string, IReadOnlyList<byte>> seenTexts = new(StringComparer.Ordinal);

        foreach (CharacterTableEntry entry in _entries)
        {
            string byteKey = ToKey(entry.Bytes);

            if (seenByteSequences.TryGetValue(byteKey, out string? previousText) && previousText != entry.Text)
            {
                issues.Add(string.Format(
                    CultureInfo.CurrentCulture, Strings.Saturn_Table_DuplicateByteSequence, FormatHex(entry.Bytes), previousText, entry.Text));
            }
            else
            {
                seenByteSequences[byteKey] = entry.Text;
            }

            if (seenTexts.TryGetValue(entry.Text, out IReadOnlyList<byte>? previousBytes) && !previousBytes.SequenceEqual(entry.Bytes))
            {
                issues.Add(string.Format(
                    CultureInfo.CurrentCulture, Strings.Saturn_Table_DuplicateText, entry.Text, FormatHex(previousBytes), FormatHex(entry.Bytes)));
            }
            else
            {
                seenTexts[entry.Text] = entry.Bytes;
            }
        }

        return issues;
    }

    private (string Text, int Consumed) DecodeNext(IReadOnlyList<byte> bytes, int position)
    {
        int maxLength = Math.Min(_maxSequenceLength, bytes.Count - position);

        for (int length = maxLength; length >= 1; length--)
        {
            string key = ToKey(bytes, position, length);
            if (_decodeMap.TryGetValue(key, out string? text))
            {
                return (text, length);
            }
        }

        throw new ArgumentException(
            string.Format(CultureInfo.CurrentCulture, Strings.Saturn_Table_UnknownByte, bytes[position].ToString("X2", CultureInfo.InvariantCulture)),
            nameof(bytes));
    }

    private (byte[] Bytes, int Consumed) EncodeNext(string text, int position)
    {
        int maxLength = text.Length - position;

        for (int length = maxLength; length >= 1; length--)
        {
            string candidate = text.Substring(position, length);
            if (_encodeMap.TryGetValue(candidate, out byte[]? bytes))
            {
                return (bytes, length);
            }
        }

        throw new ArgumentException(
            string.Format(CultureInfo.CurrentCulture, Strings.Saturn_Table_UnknownCharacter, text[position]),
            nameof(text));
    }

    private static string ToKey(IReadOnlyList<byte> bytes) => ToKey(bytes, 0, bytes.Count);

    private static string ToKey(IReadOnlyList<byte> bytes, int start, int length)
    {
        StringBuilder builder = new(length * 2);
        for (int i = 0; i < length; i++)
        {
            builder.Append(bytes[start + i].ToString("X2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    private static string FormatHex(IReadOnlyList<byte> bytes) => ToKey(bytes);
}
