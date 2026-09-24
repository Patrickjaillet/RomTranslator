// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.Modules.SegaSaturn.TextExtraction;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.TextExtraction;

public sealed class PointerEncodingTests
{
    [Fact]
    public void ResolveTargetOffset_returns_the_raw_value_for_an_absolute_pointer()
    {
        PointerEncoding encoding = new(ByteWidth: 4, IsBigEndian: true, IsRelative: false, BaseOffset: 0x1000);

        Assert.Equal(0x2000, encoding.ResolveTargetOffset(0x2000));
    }

    [Fact]
    public void ResolveTargetOffset_adds_the_base_for_a_relative_pointer()
    {
        PointerEncoding encoding = new(ByteWidth: 4, IsBigEndian: true, IsRelative: true, BaseOffset: 0x1000);

        Assert.Equal(0x1100, encoding.ResolveTargetOffset(0x100));
    }

    [Fact]
    public void ComputeRawValue_and_ResolveTargetOffset_are_inverse_operations_for_a_relative_pointer()
    {
        PointerEncoding encoding = new(ByteWidth: 4, IsBigEndian: true, IsRelative: true, BaseOffset: 0x1000);

        long raw = encoding.ComputeRawValue(0x1234);

        Assert.Equal(0x1234, encoding.ResolveTargetOffset(raw));
    }

    [Fact]
    public void ComputeRawValue_returns_the_target_directly_for_an_absolute_pointer()
    {
        PointerEncoding encoding = new(ByteWidth: 2, IsBigEndian: false, IsRelative: false, BaseOffset: 0);

        Assert.Equal(0x5678, encoding.ComputeRawValue(0x5678));
    }
}
