// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.Windows;
using System.Windows.Controls;
using RomTranslator.App.ViewModels;

namespace RomTranslator.App.Views;

/// <summary>Vue de l'éditeur de traduction générique : liste des entrées et panneau d'édition.</summary>
public partial class TranslationEditorView : UserControl
{
    /// <summary>Initialise la vue.</summary>
    public TranslationEditorView()
    {
        InitializeComponent();
    }

    private TranslationEditorViewModel ViewModel => (TranslationEditorViewModel)DataContext;

    private void OnOpenFindReplaceClick(object sender, RoutedEventArgs e)
    {
        FindReplaceViewModel viewModel = ViewModel.CreateFindReplaceViewModel();
        FindReplaceWindow window = new(viewModel) { Owner = Window.GetWindow(this) };
        window.ShowDialog();
    }

    private void OnOpenGlossaryClick(object sender, RoutedEventArgs e)
    {
        GlossaryWindow window = new(ViewModel.Glossary) { Owner = Window.GetWindow(this) };
        window.ShowDialog();
    }
}
