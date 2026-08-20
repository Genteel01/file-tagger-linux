using System;
using Avalonia.Controls;
using Avalonia.Input;
using FileTagger.ViewModels;

namespace FileTagger.Views;

public partial class EditPanel : UserControl
{
    public EditPanel()
    {
        InitializeComponent();

        TitleField.TextFilter = (search, item) =>
        {
            if (search == MainWindowViewModel.UNCHANGED_FIELD || item == MainWindowViewModel.UNCHANGED_FIELD || string.IsNullOrWhiteSpace(search))
            {
                return true;
            }

            return item?.Contains(search) ?? false;
        };
        AlbumField.TextFilter = (search, item) =>
        {
            if (search == MainWindowViewModel.UNCHANGED_FIELD || item == MainWindowViewModel.UNCHANGED_FIELD || string.IsNullOrWhiteSpace(search))
            {
                return true;
            }

            return item?.Contains(search) ?? false;
        };
    }

    private void AutoCompleteBoxFocusLost(object? sender, FocusChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.FieldChanged();
        }
    }

    private void AutoCompleteBoxEnterPressed(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is MainWindowViewModel vm)
        {
            vm.FieldChanged();
        }
    }

    private void AutoCompleteBoxFocusGained(object? sender, FocusChangedEventArgs e)
    {
        //Open the dropdown if you focused the box via navigation or pointer
        if (sender is AutoCompleteBox box && e.NavigationMethod != NavigationMethod.Unspecified)
        {
            box.IsDropDownOpen = true;
        }
    }

    private void AutoCompleteBoxDropdownClosed(object? sender, EventArgs e)
    {
        if (sender is AutoCompleteBox box)
        {
            box.Focus();
        }
    }
}