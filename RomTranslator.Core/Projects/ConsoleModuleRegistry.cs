// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RomTranslator.Core.Projects;

/// <summary>
/// Registre des modules consoles disponibles. Un module s'enregistre par <see cref="Register" /> ; le Core
/// et l'application composent la liste des modules chargés sans jamais référencer un module concret
/// (voir la séparation de dépendances : <c>Modules.*</c> dépend du Core, jamais l'inverse). Pour l'instant
/// alimenté par composition directe (un seul module, Sega Saturn) plutôt que par une découverte automatique
/// (scan d'un sous-dossier de modules), reportée à une phase ultérieure d'extensibilité multi-consoles.
/// </summary>
public sealed class ConsoleModuleRegistry
{
    private readonly Dictionary<string, IConsoleModule> _modules = new(StringComparer.Ordinal);

    /// <summary>Modules enregistrés, dans leur ordre d'enregistrement.</summary>
    public ReadOnlyCollection<IConsoleModule> Modules => new(_modules.Values.ToList());

    /// <summary>Enregistre un module.</summary>
    /// <exception cref="ArgumentException">Un module porte déjà cet identifiant.</exception>
    public void Register(IConsoleModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        if (!_modules.TryAdd(module.Id, module))
        {
            throw new ArgumentException($"Un module d'identifiant « {module.Id} » est déjà enregistré.", nameof(module));
        }
    }

    /// <summary>Recherche un module par son identifiant.</summary>
    public IConsoleModule? Find(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return _modules.GetValueOrDefault(id);
    }
}
