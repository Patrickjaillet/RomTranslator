// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;
using System.Linq;
using System.Text;
using RomTranslator.Core.Abstractions;

namespace RomTranslator.Tests.Support;

/// <summary>Table de caractères minimale (encodage ASCII 1 octet par caractère) utilisée par les tests.</summary>
internal sealed class FakeCharacterTable : ICharacterTable
{
    public string Name => "ASCII de test";

    public string Decode(IReadOnlyList<byte> bytes) => Encoding.ASCII.GetString(bytes.ToArray());

    public IReadOnlyList<byte> Encode(string text) => Encoding.ASCII.GetBytes(text);

    public IReadOnlyList<string> Validate() => System.Array.Empty<string>();
}
