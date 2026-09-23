// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RomTranslator.App.ViewModels;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.Repository;

/// <summary>
/// La bibliothèque d'icônes a trois représentations qui doivent rester identiques : les fichiers SVG
/// (source), les ressources Geometry de Icons.xaml (utilisées par l'interface) et les constantes de IconKeys.
/// </summary>
public sealed class IconLibraryTests
{
    internal const string IconsFolder = "RomTranslator.App/Assets/Icons";
    internal const string IconsDictionary = "RomTranslator.App/Resources/Icons.xaml";

    private static readonly XNamespace _svg = "http://www.w3.org/2000/svg";
    private static readonly XNamespace _presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace _xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

    private static string Normalize(string value)
    {
        return Regex.Replace(value.Trim(), @"\s+", " ");
    }

    private static Dictionary<string, string> LoadSvgIcons()
    {
        Dictionary<string, string> icons = new();

        foreach (string file in RepositoryLocator.EnumerateFiles(IconsFolder, "*.svg"))
        {
            XElement root = XDocument.Load(file).Root!;
            string data = string.Join(" ", root.Elements(_svg + "path").Select(path => Normalize((string?)path.Attribute("d") ?? string.Empty)));
            icons[Path.GetFileNameWithoutExtension(file)] = data;
        }

        return icons;
    }

    private static Dictionary<string, string> LoadXamlGeometries()
    {
        string path = Path.Combine(RepositoryLocator.Root, IconsDictionary);
        XDocument document = XDocument.Load(path);

        return document.Root!.Elements(_presentation + "Geometry")
            .ToDictionary(
                element => ((string?)element.Attribute(_xaml + "Key") ?? string.Empty).Replace("Icon.", string.Empty, StringComparison.Ordinal),
                element => Normalize(element.Value));
    }

    private static HashSet<string> LoadDeclaredKeys()
    {
        return typeof(IconKeys)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral)
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToHashSet(StringComparer.Ordinal);
    }

    [Fact]
    public void Svg_files_use_a_24_grid_and_only_path_elements()
    {
        Dictionary<string, string> icons = LoadSvgIcons();
        Assert.NotEmpty(icons);

        foreach (string file in RepositoryLocator.EnumerateFiles(IconsFolder, "*.svg"))
        {
            XElement root = XDocument.Load(file).Root!;
            string name = Path.GetFileName(file);

            Assert.Equal("0 0 24 24", (string?)root.Attribute("viewBox"));
            Assert.Empty(root.Descendants().Where(element => element.Name != _svg + "path"));
            Assert.True(root.Elements(_svg + "path").Any(), name + " ne contient aucun tracé.");
        }
    }

    [Fact]
    public void Path_data_uses_only_commands_supported_by_the_conversion()
    {
        foreach ((string name, string data) in LoadSvgIcons())
        {
            Assert.Matches(@"^[MLHVCAZ0-9\s.,\-]+$", data);
            Assert.True(data.StartsWith('M'), name + " doit commencer par une commande M.");
        }
    }

    [Fact]
    public void Icons_dictionary_matches_the_svg_files_exactly()
    {
        Dictionary<string, string> svg = LoadSvgIcons();
        Dictionary<string, string> xaml = LoadXamlGeometries();

        Assert.Equal(svg.Keys.OrderBy(key => key, StringComparer.Ordinal), xaml.Keys.OrderBy(key => key, StringComparer.Ordinal));

        foreach ((string name, string data) in svg)
        {
            Assert.True(xaml[name] == data, "Icons.xaml diffère de " + name + ".svg : recopier les attributs d des éléments path.");
        }
    }

    [Fact]
    public void Icon_keys_match_the_svg_files_exactly()
    {
        HashSet<string> declared = LoadDeclaredKeys();
        HashSet<string> files = LoadSvgIcons().Keys.ToHashSet(StringComparer.Ordinal);

        Assert.Equal(files.OrderBy(key => key, StringComparer.Ordinal), declared.OrderBy(key => key, StringComparer.Ordinal));
    }
}
