// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.Localization;

/// <summary>Garde-fou : les fichiers de ressources français et anglais exposent exactement le même jeu de clés.</summary>
public sealed class LocalizationCompletenessTests
{
    private static string LocalizationFolder => Path.Combine(RepositoryLocator.Root, "RomTranslator.Core", "Localization");

    private static IReadOnlySet<string> ReadKeys(string fileName)
    {
        XDocument document = XDocument.Load(Path.Combine(LocalizationFolder, fileName));

        return document.Root!
            .Elements("data")
            .Select(element => element.Attribute("name")!.Value)
            .ToHashSet();
    }

    [Fact]
    public void English_resources_define_exactly_the_same_keys_as_the_default_French_resources()
    {
        IReadOnlySet<string> french = ReadKeys("Strings.resx");
        IReadOnlySet<string> english = ReadKeys("Strings.en.resx");

        List<string> missingInEnglish = french.Except(english).OrderBy(key => key).ToList();
        List<string> missingInFrench = english.Except(french).OrderBy(key => key).ToList();

        Assert.Empty(missingInEnglish);
        Assert.Empty(missingInFrench);
    }

    [Fact]
    public void No_resource_value_is_empty()
    {
        foreach (string fileName in new[] { "Strings.resx", "Strings.en.resx" })
        {
            XDocument document = XDocument.Load(Path.Combine(LocalizationFolder, fileName));

            List<string> emptyKeys = document.Root!
                .Elements("data")
                .Where(element => string.IsNullOrWhiteSpace(element.Element("value")?.Value))
                .Select(element => element.Attribute("name")!.Value)
                .ToList();

            Assert.Empty(emptyKeys);
        }
    }
}
