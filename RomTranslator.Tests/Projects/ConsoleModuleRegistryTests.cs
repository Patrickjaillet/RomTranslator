// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.Core.Projects;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.Projects;

public sealed class ConsoleModuleRegistryTests
{
    [Fact]
    public void Register_makes_the_module_findable_by_its_id()
    {
        ConsoleModuleRegistry registry = new();
        FakeConsoleModule module = new("sega-saturn");

        registry.Register(module);

        Assert.Same(module, registry.Find("sega-saturn"));
    }

    [Fact]
    public void Find_returns_null_for_an_unknown_id()
    {
        ConsoleModuleRegistry registry = new();

        Assert.Null(registry.Find("unknown"));
    }

    [Fact]
    public void Register_rejects_a_duplicate_id()
    {
        ConsoleModuleRegistry registry = new();
        registry.Register(new FakeConsoleModule("sega-saturn"));

        Assert.Throws<ArgumentException>(() => registry.Register(new FakeConsoleModule("sega-saturn")));
    }

    [Fact]
    public void Register_rejects_null()
    {
        ConsoleModuleRegistry registry = new();

        Assert.Throws<ArgumentNullException>(() => registry.Register(null!));
    }

    [Fact]
    public void Modules_preserves_registration_order()
    {
        ConsoleModuleRegistry registry = new();
        FakeConsoleModule first = new("first");
        FakeConsoleModule second = new("second");

        registry.Register(first);
        registry.Register(second);

        Assert.Equal(new IConsoleModule[] { first, second }, registry.Modules);
    }
}
