// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.Core.Abstractions;

namespace RomTranslator.Tests.Support;

/// <summary>
/// Contrainte de longueur de test : la limite autorisée est la longueur encodée du texte source
/// (comportement courant d'un module console qui ne dispose pas de plus d'espace que l'original).
/// </summary>
internal sealed class FakeTranslationLengthPolicy : ITranslationLengthPolicy
{
    public TranslationLengthCheck Check(ITranslationEntry entry, ICharacterTable characterTable)
    {
        int limit = characterTable.Encode(entry.SourceText).Count;
        int encodedLength = characterTable.Encode(entry.TranslatedText).Count;

        return new TranslationLengthCheck(limit, encodedLength);
    }
}
