// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;

namespace RomTranslator.App.ViewModels;

/// <summary>Onglet de la fenêtre principale (accueil ou console).</summary>
public sealed class TabItemViewModel
{
    /// <summary>Initialise un onglet.</summary>
    /// <param name="id">Identifiant unique et stable (conservé d'une session à l'autre).</param>
    /// <param name="title">Titre affiché.</param>
    /// <param name="iconKey">Clé de l'icône (voir <see cref="IconKeys" />).</param>
    /// <param name="content">Modèle de vue du contenu ; sa vue est choisie par un modèle de données XAML.</param>
    /// <param name="isPermanent">Indique que l'onglet ne peut pas être retiré.</param>
    public TabItemViewModel(string id, string title, string iconKey, object content, bool isPermanent = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(iconKey);
        ArgumentNullException.ThrowIfNull(content);

        Id = id;
        Title = title;
        IconKey = iconKey;
        Content = content;
        IsPermanent = isPermanent;
    }

    /// <summary>Identifiant unique de l'onglet.</summary>
    public string Id { get; }

    /// <summary>Titre affiché.</summary>
    public string Title { get; }

    /// <summary>Clé de l'icône.</summary>
    public string IconKey { get; }

    /// <summary>Modèle de vue du contenu.</summary>
    public object Content { get; }

    /// <summary>Indique que l'onglet ne peut pas être retiré.</summary>
    public bool IsPermanent { get; }
}
