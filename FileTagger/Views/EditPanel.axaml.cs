using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using FileTagger.ViewModels;

namespace FileTagger.Views;

public partial class EditPanel : UserControl
{
    private readonly AutoCompleteFilterPredicate<string?> _searchFunction = (search, item) =>
    {
        if (search == MainWindowViewModel.UnchangedField || item == MainWindowViewModel.UnchangedField || string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        return item?.Contains(search) ?? false;
    };
    public EditPanel()
    {
        InitializeComponent();

        TitleField.TextFilter = _searchFunction;
        AlbumField.TextFilter = _searchFunction;
        ArtistField.TextFilter = _searchFunction;
    }

    private void AutoCompleteBoxFocusGained(object? sender, FocusChangedEventArgs e)
    {
        //Open the dropdown if you focused the box via navigation or pointer
        if (sender is AutoCompleteBox box)
        {
            box.IsDropDownOpen = true;
        }
    }

    private void AutoCompleteBoxDropdownClosed(object? sender, EventArgs e)
    {
        //The box loses keyboard focus if you select an item from the dropdown, but retains it if it closes otherwise
        if (sender is AutoCompleteBox { IsKeyboardFocusWithin: false })
        {
            SidePanel.Focus();
        }
    }

    /// <summary>
    /// When we lose focus on the main panel, register the changes.
    /// Fires whenever focus is lost on either the panel or a descendant, so we have to check the new focused element.
    /// Checks to make sure the main panel is not the new focused element or an ancestor of it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PanelFocusLost(object? sender, FocusChangedEventArgs e)
    {
        //Checking that the new focused element is not our main panel or a descendant
        if (sender is StackPanel s && e.NewFocusedElement is Control c && s != c && !s.IsLogicalAncestorOf(c))
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.FieldChanged();
            }
        }
    }

    private void ExpandButtonTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Button b)
        {
            IEnumerable<ILogical> siblings = b.GetLogicalSiblings();
            foreach (ILogical sibling in siblings)
            {
                if (sibling is AutoCompleteBox box)
                {
                    box.Focus();
                }
            }
        }
    }
}