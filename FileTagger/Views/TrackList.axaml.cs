using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using FileTagger.Controls;
using FileTagger.Converters;
using FileTagger.Statics;
using FileTagger.ViewModels;

namespace FileTagger.Views;

public partial class TrackList : UserControl
{
    private readonly Dictionary<string, (ListColumnHeader header, GridSplitter splitter)> _columnHeaders =
        new Dictionary<string, (ListColumnHeader header, GridSplitter splitter)>();
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

        foreach (KeyValuePair<string, TrackListData.ListColumnData> possibleListColumn in TrackListData.PossibleListColumns)
        {
            ListColumnHeader columnHeader = new ListColumnHeader
            {
                Text = possibleListColumn.Value.Label,
                [!ListColumnHeader.ShowUpArrowProperty] = CompiledBinding.Create<MainWindowViewModel, bool>(vm => !vm.SortDescending),
                [!ListColumnHeader.ShowDownArrowProperty] = CompiledBinding.Create<MainWindowViewModel, bool>(vm => vm.SortDescending),
                [!ListColumnHeader.ShowArrowsProperty] = CompiledBinding.Create<MainWindowViewModel, string>(vm => vm.CurrentSort,
                    converter: new SortArrowsConverter(),
                    converterParameter: possibleListColumn.Value.Sort),
                Name = possibleListColumn.Value.Sort,
            };

            columnHeader.Tapped += ColumnHeaderTapped;

            GridSplitter splitter = new GridSplitter
            {
                ResizeDirection = GridResizeDirection.Columns,
            };

            _columnHeaders.Add(possibleListColumn.Key, (columnHeader, splitter));
        }
    }

    private void SetUpGrid()
    {
        if (DataContext is not MainWindowViewModel vm) return;

        HeaderGrid.ColumnDefinitions.Clear();
        HeaderGrid.Children.Clear();


        int columnIndex = 0;

        foreach (string key in vm.ListColumnVisibilities.Keys)
        {
            if (!vm.ListColumnVisibilities[key]) continue;

            ColumnDefinition newColumn = new ColumnDefinition
            {
                MinWidth = TrackListData.MinColumnWidth,
                SharedSizeGroup = key,
                [!ColumnDefinition.WidthProperty] = CompiledBinding.Create<MainWindowViewModel, double>(window => window.ListColumnWidths[key],
                    source: vm,
                    converter: new GridSizeConverter(),
                    mode: BindingMode.TwoWay)
            };
            HeaderGrid.ColumnDefinitions.Add(newColumn);
            GridLength splitterColumnWidth = GridLength.Auto;
            ColumnDefinition splitterColumn = new ColumnDefinition(splitterColumnWidth)
            {
                SharedSizeGroup = key + TrackListData.SplitterSizeGroupSuffix
            };
            HeaderGrid.ColumnDefinitions.Add(splitterColumn);

            (ListColumnHeader columnHeader, GridSplitter splitter) = _columnHeaders[key];
            HeaderGrid.Children.Add(columnHeader);
            Grid.SetColumn(columnHeader, columnIndex++);

            HeaderGrid.Children.Add(splitter);
            Grid.SetColumn(splitter, columnIndex++);
        }
        ColumnDefinition dummyColumn = new ColumnDefinition(GridLength.Auto)
        {
            SharedSizeGroup = TrackListData.DummyPanelSizeGroup,
        };
        HeaderGrid.ColumnDefinitions.Add(dummyColumn);

        Panel dummyPanel = new Panel
        {
            Background = new SolidColorBrush(Colors.Transparent)
        };
        HeaderGrid.Children.Add(dummyPanel);
        Grid.SetColumn(dummyPanel, columnIndex);
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

        SetUpGrid();
        if (DataContext is not MainWindowViewModel vm) return;
        vm.PropertyChanged += SetUpOnVisibilityChanged;
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        if (DataContext is not MainWindowViewModel vm) return;
        vm.PropertyChanged -= SetUpOnVisibilityChanged;
    }

    private void SetUpOnVisibilityChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(MainWindowViewModel.ListColumnVisibilities))
        {
            SetUpGrid();
        }
    }

    /// <summary>
    /// Keep the previous element focused when you scroll
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
    /// Reset the selection when we tap the empty part of the ScrollViewer
    /// </summary>
    private void ScrollViewerTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is ScrollContentPresenter)
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