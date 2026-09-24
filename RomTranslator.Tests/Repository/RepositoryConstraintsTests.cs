// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.Repository;

/// <summary>Garde-fous automatiques des contraintes strictes du projet (voir le ROADMAP).</summary>
public sealed class RepositoryConstraintsTests
{
    private static readonly string[] _productionProjects =
    {
        "RomTranslator.App",
        "RomTranslator.Core",
        "RomTranslator.Core.Abstractions",
        "RomTranslator.Modules.SegaSaturn",
    };

    private static IEnumerable<string> ProductionSources()
    {
        return _productionProjects.SelectMany(project => RepositoryLocator.EnumerateFiles(project, "*.cs"));
    }

    private static IEnumerable<string> ProductionMarkup()
    {
        return _productionProjects.SelectMany(project => RepositoryLocator.EnumerateFiles(project, "*.xaml"));
    }

    private static IEnumerable<string> AppMarkupAndSources()
    {
        return ProductionMarkup().Concat(ProductionSources());
    }

    private static List<string> FindMatches(IEnumerable<string> files, Regex pattern, string? exemptFileName = null)
    {
        List<string> offences = new();

        foreach (string file in files)
        {
            if (exemptFileName is not null && Path.GetFileName(file) == exemptFileName)
            {
                continue;
            }

            string content = File.ReadAllText(file);
            foreach (Match match in pattern.Matches(content))
            {
                offences.Add(Path.GetRelativePath(RepositoryLocator.Root, file) + " : " + match.Value);
            }
        }

        return offences;
    }

    [Fact]
    public void No_bitmap_is_referenced_by_the_interface()
    {
        Regex bitmap = new(@"\.(png|jpe?g|bmp|gif|ico|tiff?|webp)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        Assert.Empty(FindMatches(ProductionMarkup(), bitmap));
    }

    [Fact]
    public void No_installer_artifact_exists_in_the_repository()
    {
        string[] patterns = { "*.msi", "*.msix", "*.msixbundle", "*.appx", "*.appxbundle", "*.wxs", "*.wixproj", "*.iss", "*.nsi", "Package.appxmanifest" };

        List<string> found = patterns
            .SelectMany(pattern => RepositoryLocator.EnumerateFiles(".", pattern))
            .Select(path => Path.GetRelativePath(RepositoryLocator.Root, path))
            .ToList();

        Assert.Empty(found);
    }

    [Fact]
    public void No_project_enables_installer_packaging()
    {
        Regex packaging = new(@"WindowsPackageType|EnableMsixTooling|GenerateAppInstallerFile|WindowsAppSDK", RegexOptions.CultureInvariant);

        Assert.Empty(FindMatches(RepositoryLocator.EnumerateFiles(".", "*.csproj").Concat(RepositoryLocator.EnumerateFiles(".", "*.props")), packaging));
    }

    [Fact]
    public void Production_code_never_writes_outside_the_application_folder()
    {
        Regex outsideStorage = new(
            @"SpecialFolder|\bRegistry\b|GetTempPath|GetTempFileName|CreateTempSubdirectory|AppData",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        Assert.Empty(FindMatches(ProductionSources(), outsideStorage));
    }

    [Fact]
    public void The_dark_theme_and_system_theme_tracking_are_never_used()
    {
        Regex systemTheme = new(@"SystemThemeWatcher|ApplySystemTheme|Theme\s*=\s*""Dark""|ThemeMode", RegexOptions.CultureInvariant);
        Regex darkTheme = new(@"ApplicationTheme\.Dark", RegexOptions.CultureInvariant);

        Assert.Empty(FindMatches(AppMarkupAndSources(), systemTheme));

        // Seul LightThemeEnforcer cite le thème sombre, pour l'annuler.
        Assert.Empty(FindMatches(AppMarkupAndSources(), darkTheme, exemptFileName: "LightThemeEnforcer.cs"));
    }

    [Fact]
    public void Application_theme_dictionary_is_light()
    {
        string appXaml = File.ReadAllText(Path.Combine(RepositoryLocator.Root, "RomTranslator.App", "App.xaml"));

        Assert.Contains("ThemesDictionary Theme=\"Light\"", appXaml);
    }

    [Fact]
    public void Every_source_file_carries_the_license_header()
    {
        const string spdx = "SPDX-License-Identifier: GPL-3.0-only";
        const string copyright = "\u00A9 2026 Patrick JAILLET";

        List<string> missing = new();
        foreach (string file in ProductionSources().Concat(ProductionMarkup()))
        {
            string head = string.Join('\n', File.ReadLines(file).Take(6));
            if (!head.Contains(spdx) || !head.Contains(copyright))
            {
                missing.Add(Path.GetRelativePath(RepositoryLocator.Root, file));
            }
        }

        Assert.Empty(missing);
    }
}
