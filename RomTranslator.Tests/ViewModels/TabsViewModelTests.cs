// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using RomTranslator.App.ViewModels;
using Xunit;

namespace RomTranslator.Tests.ViewModels;

public sealed class TabsViewModelTests
{
    private static TabItemViewModel Tab(string id, bool isPermanent = false)
    {
        return new TabItemViewModel(id, "Titre " + id, IconKeys.Console, new object(), isPermanent);
    }

    [Fact]
    public void First_added_tab_is_selected()
    {
        TabsViewModel tabs = new();

        tabs.AddTab(Tab("home"));

        Assert.Equal("home", tabs.SelectedItem?.Id);
    }

    [Fact]
    public void Adding_a_tab_keeps_the_current_selection()
    {
        TabsViewModel tabs = new();
        tabs.AddTab(Tab("home"));

        tabs.AddTab(Tab("saturn"));

        Assert.Equal("home", tabs.SelectedItem?.Id);
        Assert.Equal(2, tabs.Items.Count);
    }

    [Fact]
    public void Adding_a_duplicate_id_throws()
    {
        TabsViewModel tabs = new();
        tabs.AddTab(Tab("home"));

        Assert.Throws<InvalidOperationException>(() => tabs.AddTab(Tab("home")));
    }

    [Fact]
    public void Preferred_tab_is_selected_when_it_appears()
    {
        TabsViewModel tabs = new("saturn");
        tabs.AddTab(Tab("home"));
        Assert.Equal("home", tabs.SelectedItem?.Id);

        tabs.AddTab(Tab("saturn"));

        Assert.Equal("saturn", tabs.SelectedItem?.Id);
    }

    [Fact]
    public void Preferred_tab_is_ignored_once_the_user_has_chosen_a_tab()
    {
        TabsViewModel tabs = new("saturn");
        tabs.AddTab(Tab("home"));
        tabs.AddTab(Tab("other"));
        tabs.SelectTab("other");

        tabs.AddTab(Tab("saturn"));

        Assert.Equal("other", tabs.SelectedItem?.Id);
    }

    [Fact]
    public void SelectTab_returns_false_for_an_unknown_id()
    {
        TabsViewModel tabs = new();
        tabs.AddTab(Tab("home"));

        Assert.False(tabs.SelectTab("missing"));
        Assert.Equal("home", tabs.SelectedItem?.Id);
    }

    [Fact]
    public void SelectedItem_ignores_a_tab_that_is_not_in_the_list()
    {
        TabsViewModel tabs = new();
        tabs.AddTab(Tab("home"));

        tabs.SelectedItem = Tab("stranger");

        Assert.Equal("home", tabs.SelectedItem?.Id);
    }

    [Fact]
    public void RemoveTab_selects_the_first_tab_when_the_selected_one_is_removed()
    {
        TabsViewModel tabs = new();
        tabs.AddTab(Tab("home", isPermanent: true));
        tabs.AddTab(Tab("saturn"));
        tabs.SelectTab("saturn");

        Assert.True(tabs.RemoveTab("saturn"));

        Assert.Equal("home", tabs.SelectedItem?.Id);
        Assert.Single(tabs.Items);
    }

    [Fact]
    public void RemoveTab_refuses_a_permanent_tab()
    {
        TabsViewModel tabs = new();
        tabs.AddTab(Tab("home", isPermanent: true));

        Assert.False(tabs.RemoveTab("home"));
        Assert.Single(tabs.Items);
    }

    [Fact]
    public void Changing_the_selection_raises_PropertyChanged()
    {
        TabsViewModel tabs = new();
        tabs.AddTab(Tab("home"));
        tabs.AddTab(Tab("saturn"));

        Assert.PropertyChanged(tabs, nameof(TabsViewModel.SelectedItem), () => tabs.SelectTab("saturn"));
    }
}
