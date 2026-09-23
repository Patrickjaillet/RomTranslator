// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using RomTranslator.App.Services;
using RomTranslator.App.ViewModels;
using RomTranslator.Core.Configuration;
using RomTranslator.Core.Diagnostics;
using RomTranslator.Core.Information;
using RomTranslator.Core.Portability;

namespace RomTranslator.App;

/// <summary>Point d'entrée de l'application RomTranslator et racine de composition.</summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        PortableLocations locations = PortableLocations.FromApplicationBase();
        if (!TryPrepareStorage(locations))
        {
            Shutdown(1);
            return;
        }

        RegisterCrashHandlers(locations);

        LightThemeEnforcer themeEnforcer = new((Color)FindResource("Palette.Accent"));
        themeEnforcer.Start();

        SettingsStore settingsStore = new(locations);
        AppSettings settings = settingsStore.Load();
        ApplicationInfo info = ApplicationInfo.FromAssembly(typeof(App).Assembly);
        string languageCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToUpperInvariant();

        MainViewModel viewModel = new(info, settingsStore, settings, new ShellUrlLauncher(), Shutdown, languageCode);

        Views.MainWindow window = new(viewModel, settings.Window);
        MainWindow = window;
        window.Show();
    }

    private static bool TryPrepareStorage(PortableLocations locations)
    {
        try
        {
            locations.EnsureDirectories();
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(
                "RomTranslator ne peut pas écrire dans son dossier :\n" + locations.RootDirectory
                + "\n\nDéplacez l'application dans un dossier accessible en écriture (bureau, disque externe, etc.).",
                "RomTranslator",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }
    }

    private void RegisterCrashHandlers(PortableLocations locations)
    {
        DispatcherUnhandledException += (_, args) =>
        {
            string? reportPath = CrashReporter.TryWrite(locations, args.Exception);
            args.Handled = true;

            string details = reportPath is null
                ? "Le rapport d'erreur n'a pas pu être enregistré."
                : "Un rapport d'erreur a été enregistré dans :\n" + reportPath;

            MessageBox.Show(
                "RomTranslator a rencontré une erreur inattendue et va se fermer.\n\n" + details,
                "RomTranslator",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception exception)
            {
                CrashReporter.TryWrite(locations, exception);
            }
        };
    }
}
