// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.App.Services;
using RomTranslator.Core.Information;
using Xunit;

namespace RomTranslator.Tests.Information;

public sealed class ApplicationInfoTests
{
    [Theory]
    [InlineData("0.1.5-alpha+abc1234", "0.1.5-alpha")]
    [InlineData("1.0.0", "1.0.0")]
    [InlineData("2.3.4+build.7", "2.3.4")]
    public void ToDisplayVersion_removes_build_metadata(string version, string expected)
    {
        Assert.Equal(expected, ApplicationInfo.ToDisplayVersion(version));
    }

    [Fact]
    public void FromAssembly_reads_product_copyright_and_version()
    {
        ApplicationInfo info = ApplicationInfo.FromAssembly(typeof(ApplicationInfo).Assembly);

        Assert.Equal("RomTranslator", info.Name);
        Assert.Equal("\u00A9 2026 Patrick JAILLET", info.Copyright);
        Assert.DoesNotContain("+", info.Version);
        Assert.Matches(@"^\d+\.\d+\.\d+", info.Version);
    }

    [Theory]
    [InlineData(ProjectLinks.WebsiteUrl)]
    [InlineData(ProjectLinks.RepositoryUrl)]
    [InlineData(ProjectLinks.ReleasesUrl)]
    [InlineData(ProjectLinks.ContactUrl)]
    public void Project_links_use_a_scheme_accepted_by_the_launcher(string url)
    {
        Assert.True(ShellUrlLauncher.IsAllowed(url));
    }

    [Fact]
    public void FromAssembly_rejects_null()
    {
        Assert.Throws<ArgumentNullException>(() => ApplicationInfo.FromAssembly(null!));
    }
}
