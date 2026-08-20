using System;
using Avalonia.Controls;
using Avalonia.Input;
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