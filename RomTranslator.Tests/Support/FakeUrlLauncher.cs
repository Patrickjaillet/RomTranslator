// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Collections.Generic;
using RomTranslator.App.Services;

namespace RomTranslator.Tests.Support;

/// <summary>Service d'ouverture d'adresses qui enregistre les demandes au lieu d'ouvrir quoi que ce soit.</summary>
internal sealed class FakeUrlLauncher : IUrlLauncher
{
    private readonly bool _succeeds;

    public FakeUrlLauncher(bool succeeds = true)
    {
        _succeeds = succeeds;
    }

    public List<string> Opened { get; } = new();

    public bool TryOpen(string url)
    {
        Opened.Add(url);
        return _succeeds;
    }
}
