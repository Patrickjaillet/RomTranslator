// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.IO;
using System.Linq;
using RomTranslator.App.ViewModels;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.ViewModels;

public sealed class HexViewerViewModelTests
{
    private static string WriteFile(string directory, int length)
    {
        string path = Path.Combine(directory, "sample.bin");
        File.WriteAllBytes(path, Enumerable.Range(0, length).Select(i => (byte)(i % 256)).ToArray());
        return path;
    }

    [Fact]
    public void Constructor_loads_the_first_page()
    {
        using TemporaryDirectory temp = new();
        HexViewerViewModel viewModel = new(WriteFile(temp.FullPath, 64));

        Assert.Equal(4, viewModel.Lines.Count);
        Assert.Equal(0, viewModel.CurrentPage);
        Assert.False(viewModel.PreviousPageCommand.CanExecute(null));
    }

    [Fact]
    public void PageCount_reflects_the_file_size()
    {
        using TemporaryDirectory temp = new();
        int lineCount = HexViewerViewModel.PageSize * 2 + 5;
        HexViewerViewModel viewModel = new(WriteFile(temp.FullPath, lineCount * 16));

        Assert.Equal(3, viewModel.PageCount);
    }

    [Fact]
    public void NextPageCommand_advances_to_the_next_page()
    {
        using TemporaryDirectory temp = new();
        int lineCount = HexViewerViewModel.PageSize + 1;
        HexViewerViewModel viewModel = new(WriteFile(temp.FullPath, lineCount * 16));

        Assert.True(viewModel.NextPageCommand.CanExecute(null));
        viewModel.NextPageCommand.Execute(null);

        Assert.Equal(1, viewModel.CurrentPage);
        Assert.Single(viewModel.Lines);
        Assert.False(viewModel.NextPageCommand.CanExecute(null));
    }

    [Fact]
    public void CurrentPage_is_clamped_to_the_valid_range()
    {
        using TemporaryDirectory temp = new();
        HexViewerViewModel viewModel = new(WriteFile(temp.FullPath, 16));

        viewModel.CurrentPage = 99;

        Assert.Equal(0, viewModel.CurrentPage);
    }

    [Fact]
    public void PageIndicatorText_is_one_based()
    {
        using TemporaryDirectory temp = new();
        HexViewerViewModel viewModel = new(WriteFile(temp.FullPath, 16));

        Assert.Contains("1", viewModel.PageIndicatorText);
    }
}
