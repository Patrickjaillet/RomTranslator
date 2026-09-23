// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.Core.Binary;
using Xunit;

namespace RomTranslator.Tests.Binary;

public sealed class HexDumpLineTests
{
    [Fact]
    public void OffsetHex_is_padded_to_eight_digits()
    {
        HexDumpLine line = new(0x1A, new byte[] { 1 });

        Assert.Equal("0000001A", line.OffsetHex);
    }

    [Fact]
    public void HexColumn_joins_bytes_as_two_digit_uppercase_hexadecimal()
    {
        HexDumpLine line = new(0, new byte[] { 0x4E, 0x45, 0x53, 0x1A });

        Assert.Equal("4E 45 53 1A", line.HexColumn);
    }

    [Fact]
    public void AsciiColumn_shows_printable_characters_and_a_dot_for_the_rest()
    {
        HexDumpLine line = new(0, new byte[] { (byte)'A', 0x00, (byte)'z', 0x7F, 0x20 });

        Assert.Equal("A.z. ", line.AsciiColumn);
    }
}
