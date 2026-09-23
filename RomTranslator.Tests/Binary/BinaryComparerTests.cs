// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;
using System.IO;
using RomTranslator.Core.Binary;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.Binary;

public sealed class BinaryComparerTests
{
    private static string WriteFile(string directory, string name, byte[] content)
    {
        string path = Path.Combine(directory, name);
        File.WriteAllBytes(path, content);
        return path;
    }

    [Fact]
    public void Compare_returns_no_difference_for_identical_files()
    {
        using TemporaryDirectory temp = new();
        byte[] content = { 1, 2, 3, 4, 5 };
        string originalPath = WriteFile(temp.FullPath, "a.bin", content);
        string modifiedPath = WriteFile(temp.FullPath, "b.bin", content);

        Assert.Empty(BinaryComparer.Compare(originalPath, modifiedPath));
    }

    [Fact]
    public void Compare_detects_a_single_contiguous_difference()
    {
        using TemporaryDirectory temp = new();
        string originalPath = WriteFile(temp.FullPath, "a.bin", new byte[] { 1, 2, 3, 4, 5 });
        string modifiedPath = WriteFile(temp.FullPath, "b.bin", new byte[] { 1, 2, 9, 9, 5 });

        IReadOnlyList<BinaryDifference> differences = BinaryComparer.Compare(originalPath, modifiedPath);

        BinaryDifference only = Assert.Single(differences);
        Assert.Equal(2, only.Offset);
        Assert.Equal(new byte[] { 3, 4 }, only.OriginalBytes);
        Assert.Equal(new byte[] { 9, 9 }, only.ModifiedBytes);
    }

    [Fact]
    public void Compare_detects_several_separate_differences()
    {
        using TemporaryDirectory temp = new();
        string originalPath = WriteFile(temp.FullPath, "a.bin", new byte[] { 1, 2, 3, 4, 5, 6, 7 });
        string modifiedPath = WriteFile(temp.FullPath, "b.bin", new byte[] { 9, 2, 3, 4, 5, 6, 8 });

        IReadOnlyList<BinaryDifference> differences = BinaryComparer.Compare(originalPath, modifiedPath);

        Assert.Equal(2, differences.Count);
        Assert.Equal(0, differences[0].Offset);
        Assert.Equal(6, differences[1].Offset);
    }

    [Fact]
    public void Compare_treats_a_length_difference_as_a_trailing_difference()
    {
        using TemporaryDirectory temp = new();
        string originalPath = WriteFile(temp.FullPath, "a.bin", new byte[] { 1, 2, 3 });
        string modifiedPath = WriteFile(temp.FullPath, "b.bin", new byte[] { 1, 2, 3, 4, 5 });

        IReadOnlyList<BinaryDifference> differences = BinaryComparer.Compare(originalPath, modifiedPath);

        BinaryDifference only = Assert.Single(differences);
        Assert.Equal(3, only.Offset);
        Assert.Equal(new byte[] { 0, 0 }, only.OriginalBytes);
        Assert.Equal(new byte[] { 4, 5 }, only.ModifiedBytes);
    }

    [Fact]
    public void Compare_handles_files_larger_than_the_internal_buffer()
    {
        using TemporaryDirectory temp = new();
        byte[] original = new byte[200_000];
        byte[] modified = new byte[200_000];
        modified[150_000] = 0xFF;

        string originalPath = WriteFile(temp.FullPath, "a.bin", original);
        string modifiedPath = WriteFile(temp.FullPath, "b.bin", modified);

        IReadOnlyList<BinaryDifference> differences = BinaryComparer.Compare(originalPath, modifiedPath);

        BinaryDifference only = Assert.Single(differences);
        Assert.Equal(150_000, only.Offset);
    }
}
