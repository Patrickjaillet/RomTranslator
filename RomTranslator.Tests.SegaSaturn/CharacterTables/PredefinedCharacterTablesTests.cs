// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.Modules.SegaSaturn.CharacterTables;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.CharacterTables;

/// <summary>
/// Vérifie les tables prédéfinies livrées avec le module. Les fichiers <c>.tbl</c> sont copiés à côté de
/// l'assembly de test par le mécanisme <c>CopyToOutputDirectory</c> défini dans le csproj du module.
/// </summary>
public sealed class PredefinedCharacterTablesTests
{
    [Fact]
    public void LoadAscii_is_internally_consistent_and_covers_printable_ascii()
    {
        SaturnCharacterTable table = PredefinedCharacterTables.LoadAscii(System.AppContext.BaseDirectory);

        Assert.Empty(table.Validate());
        Assert.Equal("Hello, World!", table.Decode(table.Encode("Hello, World!")));
    }

    [Fact]
    public void LoadShiftJisKana_is_internally_consistent_and_decodes_hiragana()
    {
        SaturnCharacterTable table = PredefinedCharacterTables.LoadShiftJisKana(System.AppContext.BaseDirectory);

        Assert.Empty(table.Validate());
        // "あ" en Shift-JIS est l'octet-pair 0x82 0xA0 (vérifié par l'encodeur .NET lors de la génération de la table).
        Assert.Equal("あ", table.Decode(new byte[] { 0x82, 0xA0 }));
    }

    [Fact]
    public void LoadShiftJisKana_round_trips_a_mix_of_hiragana_and_katakana()
    {
        SaturnCharacterTable table = PredefinedCharacterTables.LoadShiftJisKana(System.AppContext.BaseDirectory);

        string text = "こんにちは";
        Assert.Equal(text, table.Decode(table.Encode(text)));
    }
}
