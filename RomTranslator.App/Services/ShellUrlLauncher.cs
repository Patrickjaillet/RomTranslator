// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.ComponentModel;
using System.Diagnostics;

namespace RomTranslator.App.Services;

/// <summary>Ouvre les adresses externes via le shell Windows, en limitant les schémas autorisés.</summary>
public sealed class ShellUrlLauncher : IUrlLauncher
{
    /// <summary>Indique si l'adresse est absolue et utilise un schéma autorisé (http, https ou mailto).</summary>
    public static bool IsAllowed(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            return false;
        }

        return uri.Scheme is "http" or "https" or "mailto";
    }

    /// <inheritdoc />
    public bool TryOpen(string url)
    {
        if (!IsAllowed(url))
        {
            return false;
        }

        try
        {
            using Process? process = Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            return true;
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException)
        {
            return false;
        }
    }
}
