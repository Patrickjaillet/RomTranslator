// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.IO;
using RomTranslator.App.ViewModels;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.ViewModels;

public sealed class BinaryDiffViewModelTests
{
    private static string WriteFile(string directory, string name, byte[] content)
    {
        string path = Path.Combine(directory, name);
        File.WriteAllBytes(path, content);
        return path;
    }

    [Fact]
    public void AreIdentical_is_true_for_identical_files()
    {
        using TemporaryDirectory temp = new();
        byte[] content = { 1, 2, 3 };
        string originalPath = WriteFile(temp.FullPath, "a.bin", content);
        string modifiedPath = WriteFile(temp.FullPath, "b.bin", content);

        BinaryDiffViewModel viewModel = new(originalPath, modifiedPath);

        Assert.True(viewModel.AreIdentical);
        Assert.Empty(viewModel.Differences);
    }

    [Fact]
    public void Differences_exposes_hexadecimal_offset_and_bytes()
    {
        using TemporaryDirectory temp = new();
        string originalPath = WriteFile(temp.FullPath, "a.bin", new byte[] { 1, 2, 3, 4 });
        string modifiedPath = WriteFile(temp.FullPath, "b.bin", new byte[] { 1, 2, 9, 4 });

        BinaryDiffViewModel viewModel = new(originalPath, modifiedPath);

        BinaryDifferenceViewModel only = Assert.Single(viewModel.Differences);
        Assert.Equal("0x00000002", only.OffsetDisplay);
        Assert.Equal(1, only.LengthInBytes);
        Assert.Equal("03", only.OriginalHex);
        Assert.Equal("09", only.ModifiedHex);
        Assert.False(viewModel.AreIdentical);
    }

    [Fact]
    public void SummaryText_reports_the_number_of_differences()
    {
        using TemporaryDirectory temp = new();
        string originalPath = WriteFile(temp.FullPath, "a.bin", new byte[] { 1, 2, 3, 4, 5, 6 });
        string modifiedPath = WriteFile(temp.FullPath, "b.bin", new byte[] { 9, 2, 3, 4, 5, 9 });

        BinaryDiffViewModel viewModel = new(originalPath, modifiedPath);

        Assert.Contains("2", viewModel.SummaryText);
    }

    [Fact]
    public void CreateIpsPatch_writes_a_valid_patch_file()
    {
        using TemporaryDirectory temp = new();
        string originalPath = WriteFile(temp.FullPath, "a.bin", new byte[] { 1, 2, 3 });
        string modifiedPath = WriteFile(temp.FullPath, "b.bin", new byte[] { 1, 9, 3 });
        string patchPath = Path.Combine(temp.FullPath, "patch.ips");

        BinaryDiffViewModel viewModel = new(originalPath, modifiedPath);
        viewModel.CreateIpsPatch(patchPath);

        Assert.True(File.Exists(patchPath));
    }
}
