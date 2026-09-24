// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Projects;

namespace RomTranslator.Tests.Support;

/// <summary>Module console minimal utilisé par les tests du registre de modules.</summary>
internal sealed class FakeConsoleModule : IConsoleModule
{
    public FakeConsoleModule(string id)
    {
        Id = id;
    }

    public string Id { get; }
    public string DisplayName => "Console de test";
    public string Description => "Module utilisé uniquement par les tests.";
    public string IconKey => "gamepad";
    public string Version => "0.0.0";
    public IRomLoader RomLoader => throw new System.NotSupportedException();
    public IRomValidator RomValidator => throw new System.NotSupportedException();
    public ITextExtractor TextExtractor => throw new System.NotSupportedException();
    public ITextInjector TextInjector => throw new System.NotSupportedException();

    public ConsoleProjectContext LoadProjectContext(string projectPath)
    {
        throw new System.NotSupportedException();
    }
}
