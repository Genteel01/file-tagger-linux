using Avalonia.Controls;
using Avalonia.Input;
using FileTagger.Controls;
using FileTagger.ViewModels;

namespace FileTagger.Views;

public partial class TrackList : UserControl
{

    /// <summary>
    /// Unselect everything in the list when we hit escape,
    /// so long as we aren't editing a TextBox, per <see cref="TrackListItem.TextBoxEnterPressed"/>
    /// </summary>
    private void Root_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            TrackListBox.UnselectAll();
        }
    }

    public TrackList()
    {
        InitializeComponent();
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
            }
        }
    }
}