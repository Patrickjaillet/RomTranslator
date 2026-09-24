// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RomTranslator.Modules.SegaSaturn.CharacterTables;

/// <summary>Une entrée éditable de l'éditeur de table de caractères : une séquence d'octets et son texte.</summary>
public sealed class CharacterTableEntryViewModel : ObservableObject
{
    private string _hexBytes;
    private string _text;

    /// <summary>Initialise une entrée éditable à partir d'une correspondance existante.</summary>
    public CharacterTableEntryViewModel(CharacterTableEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _hexBytes = ToHex(entry.Bytes);
        _text = entry.Text;
    }

    /// <summary>Initialise une entrée éditable vide, prête à être complétée par l'utilisateur.</summary>
    public CharacterTableEntryViewModel()
    {
        _hexBytes = string.Empty;
        _text = string.Empty;
    }

    /// <summary>Séquence d'octets en hexadécimal (par exemple <c>"41"</c> ou <c>"8140"</c>), telle que saisie par l'utilisateur.</summary>
    public string HexBytes
    {
        get => _hexBytes;
        set
        {
            if (SetProperty(ref _hexBytes, value ?? string.Empty))
            {
                OnPropertyChanged(nameof(IsValid));
            }
        }
    }

    /// <summary>Texte représenté par la séquence d'octets.</summary>
    public string Text
    {
        get => _text;
        set
        {
            if (SetProperty(ref _text, value ?? string.Empty))
            {
                OnPropertyChanged(nameof(IsValid));
            }
        }
    }

    /// <summary>Indique si <see cref="HexBytes" /> est une séquence hexadécimale syntaxiquement valide et non vide.</summary>
    public bool IsValid => TryParseHex(out _);

    /// <summary>Convertit l'entrée éditée en <see cref="CharacterTableEntry" />, si elle est valide.</summary>
    /// <exception cref="FormatException"><see cref="HexBytes" /> n'est pas une séquence hexadécimale valide.</exception>
    public CharacterTableEntry ToEntry()
    {
        if (!TryParseHex(out byte[] bytes))
        {
            throw new FormatException("La séquence d'octets n'est pas une valeur hexadécimale valide (longueur paire attendue).");
        }

        return new CharacterTableEntry(bytes, Text);
    }

    private bool TryParseHex(out byte[] bytes)
    {
        bytes = Array.Empty<byte>();
        string hex = _hexBytes.Trim();

        if (hex.Length == 0 || hex.Length % 2 != 0)
        {
            return false;
        }

        byte[] parsed = new byte[hex.Length / 2];
        for (int i = 0; i < parsed.Length; i++)
        {
            if (!byte.TryParse(hex.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parsed[i]))
            {
                return false;
            }
        }

        bytes = parsed;
        return true;
    }

    private static string ToHex(IReadOnlyList<byte> bytes)
    {
        return string.Concat(bytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
    }
}
