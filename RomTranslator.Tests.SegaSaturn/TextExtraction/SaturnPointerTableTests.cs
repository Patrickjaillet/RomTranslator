// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.Modules.SegaSaturn.TextExtraction;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.TextExtraction;

public sealed class SaturnPointerTableTests
{
    [Fact]
    public void ReadPointers_decodes_big_endian_absolute_pointers()
    {
        PointerEncoding encoding = new(ByteWidth: 4, IsBigEndian: true, IsRelative: false, BaseOffset: 0);
        SaturnPointerTable table = new(encoding);
        byte[] bytes = { 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x20, 0x00 };

        var pointers = table.ReadPointers(bytes, pointerTableOffset: 0x500, pointerCount: 2);

        Assert.Equal(2, pointers.Count);
        Assert.Equal(0x500, pointers[0].PointerOffset);
        Assert.Equal(0x1000, pointers[0].TargetOffset);
        Assert.Equal(0x504, pointers[1].PointerOffset);
        Assert.Equal(0x2000, pointers[1].TargetOffset);
    }

    [Fact]
    public void ReadPointers_decodes_little_endian_pointers()
    {
        PointerEncoding encoding = new(ByteWidth: 2, IsBigEndian: false, IsRelative: false, BaseOffset: 0);
        SaturnPointerTable table = new(encoding);
        byte[] bytes = { 0x34, 0x12 };

        var pointers = table.ReadPointers(bytes, pointerTableOffset: 0, pointerCount: 1);

        Assert.Equal(0x1234, pointers[0].TargetOffset);
    }

    [Fact]
    public void ReadPointers_resolves_relative_pointers_against_the_base_offset()
    {
        PointerEncoding encoding = new(ByteWidth: 2, IsBigEndian: true, IsRelative: true, BaseOffset: 0x8000);
        SaturnPointerTable table = new(encoding);
        byte[] bytes = { 0x01, 0x00 };

        var pointers = table.ReadPointers(bytes, pointerTableOffset: 0, pointerCount: 1);

        Assert.Equal(0x8100, pointers[0].TargetOffset);
    }

    [Fact]
    public void ReadPointers_throws_when_the_data_is_too_short()
    {
        PointerEncoding encoding = new(ByteWidth: 4, IsBigEndian: true, IsRelative: false, BaseOffset: 0);
        SaturnPointerTable table = new(encoding);

        Assert.Throws<ArgumentException>(() => table.ReadPointers(new byte[2], pointerTableOffset: 0, pointerCount: 1));
    }

    [Fact]
    public void EncodePointer_then_ReadPointers_round_trips_the_target_offset()
    {
        PointerEncoding encoding = new(ByteWidth: 4, IsBigEndian: true, IsRelative: true, BaseOffset: 0x6000);
        SaturnPointerTable table = new(encoding);

        var encoded = table.EncodePointer(0x6234);
        var pointers = table.ReadPointers(encoded, pointerTableOffset: 0, pointerCount: 1);

        Assert.Equal(0x6234, pointers[0].TargetOffset);
    }
}
