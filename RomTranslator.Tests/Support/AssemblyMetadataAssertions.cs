// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

namespace RomTranslator.Tests.Support;

/// <summary>
/// Vérifications communes des métadonnées d'assembly exigées pour chaque projet de la solution :
/// copyright du projet et numéro de version conforme à SemVer 2.0.0.
/// </summary>
internal static class AssemblyMetadataAssertions
{
    internal const string ExpectedCopyright = "\u00A9 2026 Patrick JAILLET";

    // Expression de référence de la spécification SemVer 2.0.0, adaptée : le préfixe MAJOR.MINOR.PATCH
    // est obligatoire, le suffixe de pré-publication (-alpha.1) et les métadonnées de build (+abc123) sont facultatifs.
    private static readonly Regex _semVerPattern = new(
        @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(-[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*)?(\+[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*)?$",
        RegexOptions.CultureInvariant);

    /// <summary>Charge un assembly de la solution à partir de son nom simple.</summary>
    internal static Assembly Load(string assemblyName)
    {
        return Assembly.Load(new AssemblyName(assemblyName));
    }

    /// <summary>Vérifie que l'assembly porte le copyright « © 2026 Patrick JAILLET ».</summary>
    internal static void AssertCopyright(Assembly assembly)
    {
        string? copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;

        Assert.Equal(ExpectedCopyright, copyright);
    }

    /// <summary>Vérifie que la version informationnelle de l'assembly respecte SemVer (MAJOR.MINOR.PATCH).</summary>
    internal static void AssertSemanticVersion(Assembly assembly)
    {
        string? version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        Assert.NotNull(version);
        Assert.Matches(_semVerPattern, version);
    }
}
