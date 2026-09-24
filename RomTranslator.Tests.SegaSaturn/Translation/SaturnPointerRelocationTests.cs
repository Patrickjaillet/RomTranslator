// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;
using RomTranslator.Modules.SegaSaturn.TextExtraction;
using RomTranslator.Modules.SegaSaturn.Translation;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.Translation;

public sealed class SaturnPointerRelocationTests
{
    private static PointerEncoding CreateEncoding() => new(ByteWidth: 4, IsBigEndian: true, IsRelative: false, BaseOffset: 0);

    [Fact]
    public void FindPointerOffsets_returns_the_known_pointers_for_a_string_offset()
    {
        SaturnPointerRelocation relocation = new(
            CreateEncoding(),
            new Dictionary<long, IReadOnlyList<long>> { [0x100] = new long[] { 0x10, 0x20 } },
            FreeSpaceOffset: 0x1000,
            FreeSpaceLength: 0x100);

        Assert.Equal(new long[] { 0x10, 0x20 }, relocation.FindPointerOffsets(0x100));
    }

    [Fact]
    public void FindPointerOffsets_returns_an_empty_list_for_an_unknown_string_offset()
    {
        SaturnPointerRelocation relocation = new(
            CreateEncoding(),
            new Dictionary<long, IReadOnlyList<long>>(),
            FreeSpaceOffset: 0x1000,
            FreeSpaceLength: 0x100);

        Assert.Empty(relocation.FindPointerOffsets(0x999));
    }
}
