// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.App.ViewModels;
using RomTranslator.Core.Information;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.ViewModels;

public sealed class AboutViewModelTests
{
    [Fact]
    public void Exposes_the_product_identity()
    {
        ApplicationInfo info = new("RomTranslator", "© 2026 Patrick JAILLET", "1.2.3");
        FakeUrlLauncher launcher = new();
        LinkItemViewModel website = new("Site officiel", "desc", IconKeys.Website, ProjectLinks.WebsiteUrl, launcher, _ => { });
        LinkItemViewModel repository = new("Code source", "desc", IconKeys.Repository, ProjectLinks.RepositoryUrl, launcher, _ => { });

        AboutViewModel viewModel = new(info, website, repository);

        Assert.Equal("RomTranslator", viewModel.ProductName);
        Assert.Equal("Version 1.2.3", viewModel.VersionText);
        Assert.Equal("© 2026 Patrick JAILLET", viewModel.CopyrightText);
        Assert.Same(website, viewModel.Website);
        Assert.Same(repository, viewModel.Repository);
    }

    [Fact]
    public void Constructor_rejects_null()
    {
        ApplicationInfo info = new("RomTranslator", "© 2026 Patrick JAILLET", "1.0.0");
        FakeUrlLauncher launcher = new();
        LinkItemViewModel link = new("Lien", "desc", IconKeys.Website, ProjectLinks.WebsiteUrl, launcher, _ => { });

        Assert.Throws<System.ArgumentNullException>(() => new AboutViewModel(null!, link, link));
        Assert.Throws<System.ArgumentNullException>(() => new AboutViewModel(info, null!, link));
        Assert.Throws<System.ArgumentNullException>(() => new AboutViewModel(info, link, null!));
    }
}
