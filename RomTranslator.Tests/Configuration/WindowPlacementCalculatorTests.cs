// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.Core.Configuration;
using Xunit;

namespace RomTranslator.Tests.Configuration;

public sealed class WindowPlacementCalculatorTests
{
    internal const double MinWidth = 960;
    internal const double MinHeight = 540;

    private static readonly WindowBounds _singleScreen = new(0, 0, 1920, 1080);

    private static WindowSettings Saved(double left, double top, double width, double height)
    {
        return new WindowSettings { Left = left, Top = top, Width = width, Height = height };
    }

    [Fact]
    public void Resolve_returns_null_when_no_bounds_are_saved()
    {
        WindowSettings incomplete = new() { Left = 10 };

        Assert.Null(WindowPlacementCalculator.Resolve(incomplete, _singleScreen, MinWidth, MinHeight));
    }

    [Fact]
    public void Resolve_keeps_valid_bounds()
    {
        WindowBounds? resolved = WindowPlacementCalculator.Resolve(Saved(100, 50, 1200, 800), _singleScreen, MinWidth, MinHeight);

        Assert.Equal(new WindowBounds(100, 50, 1200, 800), resolved);
    }

    [Fact]
    public void Resolve_moves_a_window_back_onto_the_screen()
    {
        WindowBounds? resolved = WindowPlacementCalculator.Resolve(Saved(5000, 4000, 1200, 800), _singleScreen, MinWidth, MinHeight);

        Assert.Equal(new WindowBounds(720, 280, 1200, 800), resolved);
    }

    [Fact]
    public void Resolve_supports_a_virtual_screen_with_a_negative_origin()
    {
        WindowBounds twoScreens = new(-1920, 0, 3840, 1080);

        WindowBounds? resolved = WindowPlacementCalculator.Resolve(Saved(-1800, 100, 1200, 800), twoScreens, MinWidth, MinHeight);

        Assert.Equal(new WindowBounds(-1800, 100, 1200, 800), resolved);
    }

    [Fact]
    public void Resolve_enforces_the_minimum_size()
    {
        WindowBounds? resolved = WindowPlacementCalculator.Resolve(Saved(0, 0, 100, 100), _singleScreen, MinWidth, MinHeight);

        Assert.Equal(new WindowBounds(0, 0, MinWidth, MinHeight), resolved);
    }

    [Fact]
    public void Resolve_shrinks_a_window_larger_than_the_screen()
    {
        WindowBounds? resolved = WindowPlacementCalculator.Resolve(Saved(-300, -300, 5000, 5000), _singleScreen, MinWidth, MinHeight);

        Assert.Equal(new WindowBounds(0, 0, 1920, 1080), resolved);
    }

    [Fact]
    public void Resolve_returns_null_for_non_finite_values()
    {
        Assert.Null(WindowPlacementCalculator.Resolve(Saved(double.NaN, 0, 1200, 800), _singleScreen, MinWidth, MinHeight));
        Assert.Null(WindowPlacementCalculator.Resolve(Saved(0, 0, double.PositiveInfinity, 800), _singleScreen, MinWidth, MinHeight));
    }
}
