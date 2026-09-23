using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using FileTagger.Assets.Statics;
using FileTagger.ViewModels;

namespace FileTagger.Views;

public partial class EditPanel : UserControl
{
    private readonly AutoCompleteFilterPredicate<string?> _searchFunction = (search, item) =>
    {
        if (string.IsNullOrWhiteSpace(search)) return true;
        string searchTrimmed = search.Trim();
        string? itemTrimmed = item?.Trim();
        if (searchTrimmed == Consts.UnchangedField || itemTrimmed == Consts.UnchangedField)
        {
            return true;
        }

        return itemTrimmed?.Contains(searchTrimmed, StringComparison.CurrentCultureIgnoreCase) ?? false;
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
}