// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.App.ViewModels;
using Xunit;

namespace RomTranslator.Tests.ViewModels;

public sealed class StatusBarViewModelTests
{
    [Fact]
    public void ReportMessage_updates_the_message_and_notifies()
    {
        StatusBarViewModel status = new("FR");

        Assert.PropertyChanged(status, nameof(StatusBarViewModel.Message), () => status.ReportMessage("Prêt"));
        Assert.Equal("Prêt", status.Message);
    }

    [Fact]
    public void BeginProgress_shows_the_bar_at_zero()
    {
        StatusBarViewModel status = new("FR");

        status.BeginProgress();

        Assert.True(status.IsProgressVisible);
        Assert.False(status.IsIndeterminate);
        Assert.Equal(0, status.Progress);
    }

    [Theory]
    [InlineData(-20, 0)]
    [InlineData(42.5, 42.5)]
    [InlineData(250, 100)]
    [InlineData(double.NaN, 0)]
    public void ReportProgress_is_clamped_between_0_and_100(double reported, double expected)
    {
        StatusBarViewModel status = new("FR");
        status.BeginProgress();

        status.ReportProgress(reported);

        Assert.Equal(expected, status.Progress);
    }

    [Fact]
    public void EndProgress_hides_and_resets_the_bar()
    {
        StatusBarViewModel status = new("FR");
        status.BeginProgress(isIndeterminate: true);
        status.ReportProgress(60);

        status.EndProgress();

        Assert.False(status.IsProgressVisible);
        Assert.False(status.IsIndeterminate);
        Assert.Equal(0, status.Progress);
    }

    [Fact]
    public void LanguageCode_can_change_and_notifies()
    {
        StatusBarViewModel status = new("FR");

        Assert.PropertyChanged(status, nameof(StatusBarViewModel.LanguageCode), () => status.LanguageCode = "EN");
        Assert.Equal("EN", status.LanguageCode);
    }
}
