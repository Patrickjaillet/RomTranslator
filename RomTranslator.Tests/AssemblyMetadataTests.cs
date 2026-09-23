// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests;

/// <summary>Métadonnées d'assembly des projets génériques de la solution (application, Core et Core.Abstractions).</summary>
public sealed class AssemblyMetadataTests
{
    public static TheoryData<string> SolutionAssemblyNames => new()
    {
        "RomTranslator",
        "RomTranslator.Core",
        "RomTranslator.Core.Abstractions",
    };

    [Theory]
    [MemberData(nameof(SolutionAssemblyNames))]
    public void Assembly_declares_project_copyright(string assemblyName)
    {
        AssemblyMetadataAssertions.AssertCopyright(AssemblyMetadataAssertions.Load(assemblyName));
    }

    [Theory]
    [MemberData(nameof(SolutionAssemblyNames))]
    public void Assembly_version_follows_semver(string assemblyName)
    {
        AssemblyMetadataAssertions.AssertSemanticVersion(AssemblyMetadataAssertions.Load(assemblyName));
    }
}
