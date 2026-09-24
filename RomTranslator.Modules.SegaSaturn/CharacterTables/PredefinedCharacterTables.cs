// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;

namespace RomTranslator.Modules.SegaSaturn.CharacterTables;

/// <summary>
/// Tables de caractères prédéfinies livrées avec le module, pour les encodages Saturn les plus courants
/// identifiés par la recherche technique préalable (voir <c>docs/technique/SegaSaturn.md</c> § 5) : ASCII
/// et le sous-ensemble hiragana/katakana du Shift-JIS. Ces fichiers sont copiés tels quels dans le dossier
/// de l'application (portables, éditables par l'utilisateur comme n'importe quelle table <c>.tbl</c>).
/// </summary>
public static class PredefinedCharacterTables
{
    private const string PredefinedTablesFolderName = "PredefinedTables";

    /// <summary>Nom de fichier de la table ASCII imprimable (0x20-0x7E, un octet par caractère).</summary>
    public const string AsciiFileName = "ascii.tbl";

    /// <summary>
    /// Nom de fichier de la table Shift-JIS limitée au hiragana, au katakana (pleine et demi-chasse) et à
    /// leur ponctuation associée. Ne couvre pas les kanji : un jeu qui en utilise nécessite une table dédiée.
    /// </summary>
    public const string ShiftJisKanaFileName = "shift-jis-kana.tbl";

    /// <summary>Charge la table ASCII prédéfinie.</summary>
    /// <param name="applicationDirectory">Dossier de l'application (où les fichiers <c>.tbl</c> prédéfinis sont copiés au build).</param>
    public static SaturnCharacterTable LoadAscii(string applicationDirectory)
    {
        return Load(applicationDirectory, AsciiFileName, "ASCII");
    }

    /// <summary>Charge la table Shift-JIS (hiragana/katakana) prédéfinie.</summary>
    /// <param name="applicationDirectory">Dossier de l'application (où les fichiers <c>.tbl</c> prédéfinis sont copiés au build).</param>
    public static SaturnCharacterTable LoadShiftJisKana(string applicationDirectory)
    {
        return Load(applicationDirectory, ShiftJisKanaFileName, "Shift-JIS (kana)");
    }

    private static SaturnCharacterTable Load(string applicationDirectory, string fileName, string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationDirectory);

        string path = Path.Combine(applicationDirectory, PredefinedTablesFolderName, fileName);
        CharacterTableFile file = CharacterTableFileReader.Read(path);
        return new SaturnCharacterTable(displayName, file);
    }
}
