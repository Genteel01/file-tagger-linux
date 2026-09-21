using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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

    private ScrollBar? _horizontalScrollBar;
    private ScrollBar? _verticalScrollBar;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        ListScroller.TemplateApplied += (_, args) =>
        {
            _horizontalScrollBar = args.NameScope.Find<ScrollBar>("PART_HorizontalScrollBar");
            _verticalScrollBar = args.NameScope.Find<ScrollBar>("PART_VerticalScrollBar");

            _horizontalScrollBar?.Focusable = true;
            _verticalScrollBar?.Focusable = true;
            _horizontalScrollBar?.GettingFocus += ScrollBarGettingFocus;
            _verticalScrollBar?.GettingFocus += ScrollBarGettingFocus;
        };
    }

    /// <summary>
    /// We want the ScrollBars to be Focusable so clicking them won't meet the condition in <see cref="ScrollViewerFocusGained"/>,
    /// but we don't want them to actually take focus from whatever you were focusing before, so we redirect it back
    /// </summary>
    private void ScrollBarGettingFocus(object? sender, FocusChangingEventArgs e)
    {
        e.TrySetNewFocusedElement(e.OldFocusedElement);
    }

    private void OnListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectionChanged();
        }
    }

    /// <summary>
    /// Reset the selection when we Focus the ScrollViewer
    /// </summary>
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