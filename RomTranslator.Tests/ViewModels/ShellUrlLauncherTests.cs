// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.App.Services;
using Xunit;

namespace RomTranslator.Tests.ViewModels;

public sealed class ShellUrlLauncherTests
{
    [Theory]
    [InlineData("https://patrickjaillet.github.io/RomTranslator")]
    [InlineData("http://example.org/page")]
    [InlineData("mailto:auteur@example.org")]
    public void Web_and_mail_addresses_are_allowed(string url)
    {
        Assert.True(ShellUrlLauncher.IsAllowed(url));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("pas une adresse")]
    [InlineData("file:///C:/Windows/System32/cmd.exe")]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://example.org/fichier")]
    [InlineData("C:\\Windows\\System32\\cmd.exe")]
    public void Other_schemes_and_malformed_input_are_rejected(string url)
    {
        Assert.False(ShellUrlLauncher.IsAllowed(url));
    }

    [Fact]
    public void TryOpen_refuses_a_disallowed_address_without_starting_anything()
    {
        Assert.False(new ShellUrlLauncher().TryOpen("file:///C:/Windows/System32/cmd.exe"));
    }
}
