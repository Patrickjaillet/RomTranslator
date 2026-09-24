// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;
using System.Text;
using RomTranslator.Modules.SegaSaturn.Disc;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.Disc;

public sealed class SaturnIpBinTests
{
    private static byte[] BuildHeader(string hardwareId = "SEGA SEGASATURN ", string title = "TEST GAME")
    {
        byte[] header = new byte[SaturnIpBin.HeaderRegionSize];
        Array.Fill(header, (byte)' ');

        WriteAscii(header, 0x000, hardwareId, 16);
        WriteAscii(header, 0x010, "SEGA ENTERPRISES", 16);
        WriteAscii(header, 0x020, "T-000000  ", 10);
        WriteAscii(header, 0x02A, "V1.000", 6);
        WriteAscii(header, 0x030, "20260101", 8);
        WriteAscii(header, 0x038, "CD-1/1  ", 8);
        WriteAscii(header, 0x040, "JTUE      ", 10);
        WriteAscii(header, 0x050, "J               ", 16);
        WriteAscii(header, 0x060, title, 112);

        return header;
    }

    private static void WriteAscii(byte[] buffer, int offset, string value, int length)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(value.PadRight(length).Substring(0, length));
        Array.Copy(bytes, 0, buffer, offset, length);
    }

    [Fact]
    public void Parse_reads_every_field_at_its_verified_offset()
    {
        SaturnIpBinHeader header = SaturnIpBin.Parse(BuildHeader());

        Assert.Equal("SEGA SEGASATURN", header.HardwareId);
        Assert.Equal("SEGA ENTERPRISES", header.MakerId);
        Assert.Equal("T-000000", header.ProductNumber);
        Assert.Equal("V1.000", header.Version);
        Assert.Equal("20260101", header.ReleaseDate);
        Assert.Equal("CD-1/1", header.DeviceInformation);
        Assert.Equal("JTUE", header.AreaSymbols);
        Assert.Equal("J", header.Peripherals);
        Assert.Equal("TEST GAME", header.GameTitle);
    }

    [Fact]
    public void Parse_trims_trailing_spaces_from_every_field()
    {
        SaturnIpBinHeader header = SaturnIpBin.Parse(BuildHeader(title: "SHORT"));

        Assert.Equal("SHORT", header.GameTitle);
        Assert.DoesNotContain(' ', header.GameTitle);
    }

    [Fact]
    public void Parse_throws_when_the_input_is_shorter_than_the_header_region()
    {
        Assert.Throws<InvalidDataException>(() => SaturnIpBin.Parse(new byte[10]));
    }

    [Fact]
    public void Parse_rejects_null()
    {
        Assert.Throws<ArgumentNullException>(() => SaturnIpBin.Parse(null!));
    }

    [Fact]
    public void Parse_does_not_validate_the_hardware_identifier_itself()
    {
        // La validation de l'identifiant matériel est la responsabilité de SaturnRomValidator/SaturnRomLoader,
        // pas de SaturnIpBin.Parse, qui se contente de lire les octets tels quels.
        SaturnIpBinHeader header = SaturnIpBin.Parse(BuildHeader(hardwareId: "NOT A SATURN ROM"));

        Assert.Equal("NOT A SATURN ROM", header.HardwareId);
    }
}
