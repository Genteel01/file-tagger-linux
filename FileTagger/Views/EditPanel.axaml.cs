using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using FileTagger.ViewModels;

namespace FileTagger.Views;

public partial class EditPanel : UserControl
{
    private readonly AutoCompleteFilterPredicate<string?> _searchFunction = (search, item) =>
    {
        if (search == EditPanelViewModel.UnchangedField || item == EditPanelViewModel.UnchangedField || string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        return item?.Contains(search, StringComparison.CurrentCultureIgnoreCase) ?? false;
    };
    public EditPanel()
    {
        InitializeComponent();

        TitleField.TextFilter = _searchFunction;
        AlbumField.TextFilter = _searchFunction;
        ArtistField.TextFilter = _searchFunction;
        YearField.TextFilter = _searchFunction;
        TrackNumberField.TextFilter = _searchFunction;
        GenreField.TextFilter = _searchFunction;
        CommentField.TextFilter = _searchFunction;
        AlbumArtistField.TextFilter = _searchFunction;
        ComposerField.TextFilter = _searchFunction;
        DiscNumberField.TextFilter = _searchFunction;
    }

    private void AutoCompleteBoxDropdownClosed(object? sender, EventArgs e)
    {
        //The box loses keyboard focus if you select an item from the dropdown, but retains it if it closes otherwise
        if (sender is AutoCompleteBox { IsKeyboardFocusWithin: false })
        {
            SidePanel.Focus();
        }
    }

    private void AutoCompleteBoxNumberFieldTextChanged(object? sender, EventArgs e)
    {
        if (sender is AutoCompleteBox box)
        {
            if (box.Text != EditPanelViewModel.UnchangedField)
            {
                box.Text = string.Concat((box.Text ?? "").Where(c => char.IsDigit(c)));
            }
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
        if (sender is not StackPanel s) return;
        if (e.NewFocusedElement is not Control c) return;
        //Checking that the new focused element is not our main panel or a descendant
        if (s != c && !s.IsLogicalAncestorOf(c))
        {
            if (DataContext is EditPanelViewModel vm)
            {
                vm.StoreFieldChanges();
            }
        }
    }

    /// <summary>
    /// Manually focusing the panel when we tap one of our image buttons.
    /// Without this there was a bug when clicking the button when the previous focus was on the TrackList
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ImageButtonTapped(object? sender, TappedEventArgs e)
    {
        SidePanel.Focus();
    }

    /// <summary>
    /// When we open the context menu, update whether we can paste
    /// </summary>
    private async void OpeningContextMenu(object? sender, CancelEventArgs e)
    {
        if (DataContext is EditPanelViewModel vm)
        {
            await vm.UpdatePasteVisibility();
        }
    }
}