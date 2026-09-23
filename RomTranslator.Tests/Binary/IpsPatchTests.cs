// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.IO;
using RomTranslator.Core.Binary;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.Binary;

public sealed class IpsPatchTests
{
    private const int _headerLength = 5;
    private const int _footerLength = 3;

    private static string WriteFile(string directory, string name, byte[] content)
    {
        string path = Path.Combine(directory, name);
        File.WriteAllBytes(path, content);
        return path;
    }

    [Fact]
    public void Create_then_Apply_round_trips_a_simple_modification()
    {
        using TemporaryDirectory temp = new();
        byte[] original = { 1, 2, 3, 4, 5, 6, 7, 8 };
        byte[] modified = { 1, 2, 9, 9, 5, 6, 7, 8 };
        string originalPath = WriteFile(temp.FullPath, "original.bin", original);
        string modifiedPath = WriteFile(temp.FullPath, "modified.bin", modified);
        string patchPath = Path.Combine(temp.FullPath, "patch.ips");
        string outputPath = Path.Combine(temp.FullPath, "output.bin");

        IpsPatch.Create(originalPath, modifiedPath, patchPath);
        IpsPatch.Apply(originalPath, patchPath, outputPath);

        Assert.Equal(modified, File.ReadAllBytes(outputPath));
    }

    [Fact]
    public void Create_writes_the_PATCH_header_and_EOF_footer()
    {
        using TemporaryDirectory temp = new();
        string originalPath = WriteFile(temp.FullPath, "original.bin", new byte[] { 1, 2, 3 });
        string modifiedPath = WriteFile(temp.FullPath, "modified.bin", new byte[] { 1, 9, 3 });
        string patchPath = Path.Combine(temp.FullPath, "patch.ips");

        IpsPatch.Create(originalPath, modifiedPath, patchPath);
        byte[] patch = File.ReadAllBytes(patchPath);

        Assert.Equal("PATCH", System.Text.Encoding.ASCII.GetString(patch, 0, 5));
        Assert.Equal("EOF", System.Text.Encoding.ASCII.GetString(patch, patch.Length - 3, 3));
    }

    [Fact]
    public void Create_produces_no_records_for_identical_files()
    {
        using TemporaryDirectory temp = new();
        byte[] content = { 1, 2, 3 };
        string originalPath = WriteFile(temp.FullPath, "original.bin", content);
        string modifiedPath = WriteFile(temp.FullPath, "modified.bin", content);
        string patchPath = Path.Combine(temp.FullPath, "patch.ips");

        IpsPatch.Create(originalPath, modifiedPath, patchPath);

        Assert.Equal(_headerLength + _footerLength, File.ReadAllBytes(patchPath).Length);
    }

    [Fact]
    public void Create_splits_a_difference_larger_than_the_record_size_limit()
    {
        using TemporaryDirectory temp = new();
        byte[] original = new byte[0x20000];
        byte[] modified = new byte[0x20000];
        for (int i = 0x100; i < 0x1FF00; i++)
        {
            modified[i] = 0xFF;
        }

        string originalPath = WriteFile(temp.FullPath, "original.bin", original);
        string modifiedPath = WriteFile(temp.FullPath, "modified.bin", modified);
        string patchPath = Path.Combine(temp.FullPath, "patch.ips");
        string outputPath = Path.Combine(temp.FullPath, "output.bin");

        IpsPatch.Create(originalPath, modifiedPath, patchPath);
        IpsPatch.Apply(originalPath, patchPath, outputPath);

        Assert.Equal(modified, File.ReadAllBytes(outputPath));
    }

    [Fact]
    public void Create_throws_when_a_difference_is_beyond_the_maximum_offset()
    {
        using TemporaryDirectory temp = new();
        long beyondLimit = IpsPatch.MaxOffset + 10;
        byte[] original = new byte[beyondLimit + 1];
        byte[] modified = new byte[beyondLimit + 1];
        modified[beyondLimit] = 1;

        string originalPath = WriteFile(temp.FullPath, "original.bin", original);
        string modifiedPath = WriteFile(temp.FullPath, "modified.bin", modified);
        string patchPath = Path.Combine(temp.FullPath, "patch.ips");

        Assert.Throws<NotSupportedException>(() => IpsPatch.Create(originalPath, modifiedPath, patchPath));
    }

    [Fact]
    public void Apply_throws_on_a_file_without_the_PATCH_header()
    {
        using TemporaryDirectory temp = new();
        string sourcePath = WriteFile(temp.FullPath, "source.bin", new byte[] { 1, 2, 3 });
        string patchPath = WriteFile(temp.FullPath, "invalid.ips", new byte[] { 0, 1, 2, 3, 4 });
        string outputPath = Path.Combine(temp.FullPath, "output.bin");

        Assert.Throws<InvalidDataException>(() => IpsPatch.Apply(sourcePath, patchPath, outputPath));
    }

    [Fact]
    public void Create_then_Apply_extends_the_original_image_when_the_modified_one_is_longer()
    {
        using TemporaryDirectory temp = new();
        byte[] original = { 1, 2, 3 };
        byte[] modified = { 1, 2, 3, 4, 5 };
        string originalPath = WriteFile(temp.FullPath, "original.bin", original);
        string modifiedPath = WriteFile(temp.FullPath, "modified.bin", modified);
        string patchPath = Path.Combine(temp.FullPath, "patch.ips");
        string outputPath = Path.Combine(temp.FullPath, "output.bin");

        IpsPatch.Create(originalPath, modifiedPath, patchPath);
        IpsPatch.Apply(originalPath, patchPath, outputPath);

        Assert.Equal(modified, File.ReadAllBytes(outputPath));
    }
}
