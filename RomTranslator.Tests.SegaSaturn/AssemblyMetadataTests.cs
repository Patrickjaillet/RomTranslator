// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn;

/// <summary>Métadonnées d'assembly du module Sega Saturn.</summary>
public sealed class AssemblyMetadataTests
{
    internal const string ModuleAssemblyName = "RomTranslator.Modules.SegaSaturn";

    [Fact]
    public void Module_assembly_declares_project_copyright()
    {
        AssemblyMetadataAssertions.AssertCopyright(AssemblyMetadataAssertions.Load(ModuleAssemblyName));
    }

    [Fact]
    public void Module_assembly_version_follows_semver()
    {
        AssemblyMetadataAssertions.AssertSemanticVersion(AssemblyMetadataAssertions.Load(ModuleAssemblyName));
    }
}
