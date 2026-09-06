using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using FileTagger.Controls;
using FileTagger.Extensions;
using FileTagger.Models;
using FileTagger.Services;
using FileTagger.ViewModels;

namespace FileTagger.Views;

public partial class TrackList : UserControl
{
    /// <summary>
    /// The <see cref="IPreferenceService"/> loaded with Dependency Injected used for storing and retrieving <see cref="Preferences"/>
    /// </summary>
    private IPreferenceService _preferenceService;

    /// <summary>
    /// All the panels in the title row
    /// </summary>
    private readonly List<ListColumnHeader> _headerPanels = [];

    /// <summary>
    /// Unselect everything in the list when we hit escape,
    /// so long as we aren't editing a TextBox, per <see cref="TextBoxEnterPressed"/>
    /// </summary>
    private void Root_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            TrackListBox.UnselectAll();
        }
    }

    /// <summary>
    /// When we hit Enter or Escape on a TextBox, stop editing it and update the ViewModel
    /// Stops the even propagating so we don't unselect the list with <see cref="Root_OnKeyDown"/>
    /// </summary>
    private void TextBoxEnterPressed(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Escape) || sender is not Control c) return;
        //Prevent the event from going further, so we don't trigger Root_OnKeyDown
        e.Handled = true;
        ListBoxItem? listBoxItem = c.FindAncestorOfType<ListBoxItem>();
        listBoxItem?.Focus();
    }

    public TrackList(IPreferenceService preferenceService)
    {
        _preferenceService = preferenceService ?? throw new ArgumentNullException(nameof(preferenceService));
        InitializeComponent();
        //Get initial sort values
        Preferences preferences = _fileService.PreferenceData;
        string initialSort = preferences.SortOrder.Item1;
        bool initialDescending = preferences.SortOrder.Item2;
        Preferences preferences = _preferenceService.PreferenceData;
        foreach (Control control in ListGridTitle.Children)
        {
            if(control is ListColumnHeader columnHeader)
            {
                _headerPanels.Add(columnHeader);
                //For each column header, set up its sorting arrows when it loads
                bool isCorrectPanel = columnHeader.Name == initialSort;
                columnHeader.Loaded += (_, _) =>
                {
                    DisplaySortIndicator(columnHeader, isCorrectPanel && !initialDescending, isCorrectPanel && initialDescending);
                };
            }
        }
    }

    private void OnListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectionChanged();
        }
    }

    private void ScrollViewerFocusGained(object? sender, FocusChangedEventArgs e)
    {
        if (e.NewFocusedElement == sender)
        {
            TrackListBox.UnselectAll();
        }
    }

    private void ValidateNumberOnlyField(object? sender, FocusChangedEventArgs e)
    {
        TextBoxFocusLost(sender, e);
        // Allow only digits
        if (sender is not TextBox textBox) return;

        string newText = textBox.Text ?? "";
        for (int i = newText.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(newText[i]))
            {
                newText = newText.Remove(i, 1);
            }
        }
        textBox.Text = newText;
    }

    /// <summary>
    /// When a <see cref="TextBox"/> loses focus, switch to the associated <see cref="TextBlock"/>
    /// </summary>
    private void TextBoxFocusLost(object? sender, FocusChangedEventArgs e)
    {
        bool newFocusIsRightClickMenu = e.NewFocusedElement is MenuItem;
        if (sender is TextBox textBox && !newFocusIsRightClickMenu)
        {
            string searchName = textBox.Name?.Replace("Box", "Block") ?? "";

            IEnumerable<ILogical> siblings = textBox.GetLogicalSiblings();
            TextBlock? siblingBlock = null;
            foreach (ILogical sibling in siblings)
            {
                if (sibling is TextBlock block && block.Name == searchName)
                {
                    siblingBlock = block;
                }
            }

            if (siblingBlock != null)
            {
                siblingBlock.IsVisible = true;
                siblingBlock.IsEnabled = true;
                textBox.IsVisible = false;
                textBox.IsEnabled = false;
                if (DataContext is MainWindowViewModel vm)
                {
                    vm.SelectionChanged();
                }
            }
        }
    }

    /// <summary>
    /// When a <see cref="TextBlock"/> is double-tapped, switch to the associated <see cref="TextBox"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBlockDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not TextBlock textBlock) return;

        string searchName = textBlock.Name?.Replace("Block", "Box") ?? "";
        IEnumerable<ILogical> siblings = textBlock.GetLogicalSiblings();
        TextBox? siblingBox = null;
        foreach (ILogical sibling in siblings)
        {
            if (sibling is TextBox box && box.Name == searchName)
            {
                siblingBox = box;
            }
        }

        if (siblingBox != null)
        {
            textBlock.IsVisible = false;
            textBlock.IsEnabled = false;
            siblingBox.IsVisible = true;
            siblingBox.IsEnabled = true;
            siblingBox.SelectAll();
            siblingBox.Focus();
        }
    }

    /// <summary>
    /// Sorts the list when a column header is tapped, using the sort order laid out in the axaml
    /// </summary>
    private void ColumnHeaderTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not ListColumnHeader header) return;

        string? fieldName = header.Name;
        if (fieldName != null)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.SortTracks(fieldName, true);
                DisplaySortIndicators(vm.CurrentSort, vm.SortDescending);
            }
        }
    }

    /// <summary>
    /// Goes through each header panel and displays the correct arrow icons
    /// </summary>
    private void DisplaySortIndicators(string currentSort, bool isDescending)
    {
        foreach (ListColumnHeader header in _headerPanels)
        {
            bool isCorrectPanel = header.Name == currentSort;
            DisplaySortIndicator(header, isCorrectPanel && !isDescending, isCorrectPanel && isDescending);
        }
    }

    /// <summary>
    /// Displays the arrow icons for a single header
    /// </summary>
    private void DisplaySortIndicator(ListColumnHeader header, bool showAscending, bool showDescending)
    {
        List<PathIcon> arrows = header.GetVisualDescendants<PathIcon>().ToList();
        foreach (PathIcon arrow in arrows)
        {
            if (arrow.Tag is "Up")
            {
                arrow.IsVisible = showAscending;
            }
            else if (arrow.Tag is "Down")
            {
                arrow.IsVisible = showDescending;
            }
        }
    }
}