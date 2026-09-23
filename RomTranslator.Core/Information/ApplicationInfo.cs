// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Reflection;

namespace RomTranslator.Core.Information;

/// <summary>Identité de l'application, lue dans les métadonnées de son assembly.</summary>
/// <param name="Name">Nom du produit.</param>
/// <param name="Copyright">Mention de copyright.</param>
/// <param name="Version">Version SemVer, sans métadonnées de build.</param>
public sealed record ApplicationInfo(string Name, string Copyright, string Version)
{
    private const string FallbackName = "RomTranslator";

    /// <summary>Lit l'identité de l'application dans un assembly.</summary>
    public static ApplicationInfo FromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        string name = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? FallbackName;
        string copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty;
        string version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString(3)
            ?? "0.0.0";

        return new ApplicationInfo(name, copyright, ToDisplayVersion(version));
    }

    /// <summary>Retire les métadonnées de build (suffixe commençant par « + ») d'un numéro de version.</summary>
    public static string ToDisplayVersion(string version)
    {
        ArgumentNullException.ThrowIfNull(version);

        int separator = version.IndexOf('+', StringComparison.Ordinal);
        return separator >= 0 ? version[..separator] : version;
    }
}
