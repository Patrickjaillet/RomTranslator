// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using RomTranslator.App.Services;
using RomTranslator.App.ViewModels;
using RomTranslator.Core.Configuration;
using RomTranslator.Core.Diagnostics;
using RomTranslator.Core.Information;
using RomTranslator.Core.Localization;
using RomTranslator.Core.Portability;

namespace RomTranslator.App;

/// <summary>Point d'entrée de l'application RomTranslator et racine de composition.</summary>
public partial class App : Application
{
    private static readonly CompositeFormat StorageNotWritableFormat = CompositeFormat.Parse(Strings.Startup_StorageNotWritable);
    private static readonly CompositeFormat CrashReportSavedFormat = CompositeFormat.Parse(Strings.Startup_CrashReportSaved);
    private static readonly CompositeFormat UnhandledExceptionFormat = CompositeFormat.Parse(Strings.Startup_UnhandledException);

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

        CultureInfo culture = LanguageSelector.Resolve(settings.LanguageCode, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;

        ApplicationInfo info = ApplicationInfo.FromAssembly(typeof(App).Assembly);
        string languageCode = culture.TwoLetterISOLanguageName.ToUpperInvariant();

        Views.MainWindow? window = null;
        MainViewModel? viewModel = null;
        viewModel = new MainViewModel(
            info,
            settingsStore,
            settings,
            new ShellUrlLauncher(),
            Shutdown,
            languageCode,
            locations.RootDirectory,
            locations.ProjectsDirectory,
            locations.TempDirectory,
            openSettings: () => Views.MainWindow.ShowSettings(window!, settingsStore, settings, locations),
            openAbout: () => Views.MainWindow.ShowAbout(window!, viewModel!),
            openNewSaturnProjectWizard: wizard => Views.MainWindow.ShowNewSaturnProjectWizard(window!, wizard),
            promptOpenProjectPath: () => Views.MainWindow.PromptOpenProjectPath(window!),
            openCharacterTableEditor: editorViewModel => Views.MainWindow.ShowCharacterTableEditor(window!, editorViewModel),
            promptSaveTranslatedRomPath: () => Views.MainWindow.PromptSaveTranslatedRomPath(window!),
            promptSaveIpsPatchPath: () => Views.MainWindow.PromptSaveIpsPatchPath(window!));

        window = new Views.MainWindow(viewModel, settings.Window);
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
                string.Format(CultureInfo.CurrentCulture, StorageNotWritableFormat, locations.RootDirectory),
                Strings.Application_Title,
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
                ? Strings.Startup_CrashReportNotSaved
                : string.Format(CultureInfo.CurrentCulture, CrashReportSavedFormat, reportPath);

            MessageBox.Show(
                string.Format(CultureInfo.CurrentCulture, UnhandledExceptionFormat, details),
                Strings.Application_Title,
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
