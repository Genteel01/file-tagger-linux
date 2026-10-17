using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using FileTagger.Extensions;
using FileTagger.Statics;
using FileTagger.ViewModels;

namespace FileTagger.Views;

public partial class TrackListItem : UserControl
{
    public TrackListItem()
    {
        InitializeComponent();
    }

    /// <summary>
    /// When we hit Enter or Escape on a TextBox, stop editing it and update the ViewModel
    /// Stops the event propagating so we don't unselect the list with <see cref="TrackList.Root_OnKeyDown"/>
    /// </summary>
    private void TextBoxEnterPressed(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Escape) || sender is not Control c) return;
        //Prevent the event from going further, so we don't trigger Root_OnKeyDown
        e.Handled = true;
        ListBoxItem? listBoxItem = c.FindAncestorOfType<ListBoxItem>();
        listBoxItem?.Focus();
    }

    /// <summary>
    /// When a <see cref="TextBox"/> loses focus, switch to the associated <see cref="TextBlock"/>
    /// </summary>
    private void TextBoxFocusLost(object? sender, FocusChangedEventArgs e)
    {
        bool newFocusIsRightClickMenu = e.NewFocusedElement is MenuItem;
        if (sender is TextBox textBox && !newFocusIsRightClickMenu)
        {
            TextBlock? siblingBlock = textBox.GetFirstSibling<TextBlock>();

            if (siblingBlock == null) return;

            siblingBlock.IsVisible = true;
            siblingBlock.IsEnabled = true;
            textBox.IsVisible = false;
            textBox.IsEnabled = false;
            ListBox? listBox = textBox.FindAncestorOfType<ListBox>();
            if (listBox?.DataContext is MainWindowViewModel vm)
            {
                vm.SelectionChanged();
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

        TextBox? siblingBox = textBlock.GetFirstSibling<TextBox>();

        if (siblingBox == null) return;

        textBlock.IsVisible = false;
        textBlock.IsEnabled = false;
        siblingBox.IsVisible = true;
        siblingBox.IsEnabled = true;
        siblingBox.SelectAll();
        siblingBox.Focus();
    }

    /// <summary>
    /// The first time the Grid is tapped, show all the TextBoxes so their size can be calculated.
    /// This is needed to prevent a bug where the ScrollViewer doesn't scroll to the TextBox when it is focused.
    /// Removed itself at the end because we only need it to run once
    /// </summary>
    private void GridTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Grid item) return;
        IEnumerable<Visual> children = item.GetVisualChildren();
        foreach (Visual child in children)
        {
            if (child is TextBox box)
            {
                box.SizeChanged += TextBoxSizeChanged;
                box.IsVisible = true;
            }
        }
        item.Tapped -= GridTapped;
    }


    /// <summary>
    /// After the size is initially calculated, hide the TextBox again and remove this handler
    /// </summary>
    private void TextBoxSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (sender is not TextBox box) return;
        box.IsVisible = false;
        box.SizeChanged -= TextBoxSizeChanged;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SetUpGrid();
        if (Parent?.Parent?.DataContext is not MainWindowViewModel vm) return;
        vm.PropertyChanged += SetUpOnVisibilityChanged;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        if (Parent?.Parent?.DataContext is not MainWindowViewModel vm) return;
        vm.PropertyChanged -= SetUpOnVisibilityChanged;
    }

    private void SetUpOnVisibilityChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(MainWindowViewModel.ListColumnVisibilities))
        {
            SetUpGrid();
        }
    }

    private void SetUpGrid()
    {
        if (Parent?.Parent?.DataContext is not MainWindowViewModel vm) return;

        TrackGrid.ColumnDefinitions.Clear();

        int columnIndex = 0;
        foreach (string key in vm.ListColumnVisibilities.Keys)
        {
            if (!vm.ListColumnVisibilities[key]) continue;

            ColumnDefinition newColumn = new ColumnDefinition(GridLength.Auto)
            {
                SharedSizeGroup = key,
            };
            TrackGrid.ColumnDefinitions.Add(newColumn);
            GridLength splitterColumnWidth = GridLength.Auto;
            ColumnDefinition splitterColumn = new ColumnDefinition(splitterColumnWidth)
            {
                SharedSizeGroup = key + TrackListData.SplitterSizeGroupSuffix
            };
            TrackGrid.ColumnDefinitions.Add(splitterColumn);

            CompiledBinding binding;

            switch (key)
            {
                case nameof(TrackViewModel.Title):
                    binding = CompiledBinding.Create<TrackViewModel, string>(track => track.Title);
                    break;
                case nameof(TrackViewModel.Album):
                    binding = CompiledBinding.Create<TrackViewModel, string>(track => track.Album);
                    break;
                case nameof(TrackViewModel.Artist):
                    binding = CompiledBinding.Create<TrackViewModel, string>(track => track.Artist);
                    break;
                case nameof(TrackViewModel.AlbumArtist):
                    binding = CompiledBinding.Create<TrackViewModel, string>(track => track.AlbumArtist);
                    break;
                case nameof(TrackViewModel.TrackNumber):
                    binding = CompiledBinding.Create<TrackViewModel, int?>(track => track.TrackNumber);
                    break;
                case nameof(TrackViewModel.DiscNumber):
                    binding = CompiledBinding.Create<TrackViewModel, int?>(track => track.DiscNumber);
                    break;
                case nameof(TrackViewModel.Year):
                    binding = CompiledBinding.Create<TrackViewModel, int?>(track => track.Year);
                    break;
                case nameof(TrackViewModel.Genre):
                    binding = CompiledBinding.Create<TrackViewModel, string>(track => track.Genre);
                    break;
                case nameof(TrackViewModel.Composer):
                    binding = CompiledBinding.Create<TrackViewModel, string>(track => track.Composer);
                    break;
                case nameof(TrackViewModel.Comment):
                    binding = CompiledBinding.Create<TrackViewModel, string>(track => track.Comment);
                    break;
                case nameof(TrackViewModel.Path):
                    binding = CompiledBinding.Create<TrackViewModel, string>(track => track.Path);
                    break;
                case nameof(TrackViewModel.CreatedDateFormatted):
                    binding = CompiledBinding.Create<TrackViewModel, string?>(track => track.CreatedDateFormatted);
                    break;
                case nameof(TrackViewModel.ModifiedDateFormatted):
                    binding = CompiledBinding.Create<TrackViewModel, string?>(track => track.ModifiedDateFormatted);
                    break;
                case nameof(TrackViewModel.Duration):
                    binding = CompiledBinding.Create<TrackViewModel, string>(track => track.Duration);
                    break;
                case nameof(TrackViewModel.Bitrate):
                    binding = CompiledBinding.Create<TrackViewModel, string>(track => track.Bitrate);
                    break;
                case nameof(TrackViewModel.SampleRate):
                    binding = CompiledBinding.Create<TrackViewModel, string>(track => track.SampleRate);
                    break;
                case nameof(TrackViewModel.AudioFormat):
                    binding = CompiledBinding.Create<TrackViewModel, string>(track => track.AudioFormat);
                    break;
                default:
                    binding = CompiledBinding.Create<TrackViewModel, string>(track => "");
                    break;
            }


            Panel columnPanel = new Panel();
            TextBlock columnTextBlock = new TextBlock
            {
                [!TextBlock.TextProperty] = binding,
            };
            columnTextBlock.DoubleTapped += TextBlockDoubleTapped;
            columnPanel.Children.Add(columnTextBlock);
            TextBox columnTextBox = new TextBox
            {
                IsVisible = false,
                IsEnabled = false,
                [!TextBox.TextProperty] = binding
            };
            columnTextBox.LostFocus += TextBoxFocusLost;
            columnTextBox.KeyDown += TextBoxEnterPressed;
            columnPanel.Children.Add(columnTextBox);

            Grid.SetColumn(columnPanel, columnIndex);
            TrackGrid.Children.Add(columnPanel);
            columnIndex += 2;
        }

        TrackGrid.ColumnDefinitions[^1].Width = new GridLength(0, GridUnitType.Pixel);
        TrackGrid.ColumnDefinitions[^1].SharedSizeGroup = null;

        ColumnDefinition dummyColumn = new ColumnDefinition(GridLength.Auto)
        {
            SharedSizeGroup = TrackListData.DummyPanelSizeGroup,
        };
        TrackGrid.ColumnDefinitions.Add(dummyColumn);

        Panel dummyPanel = new Panel
        {
            Classes = { TrackListData.DummyPanelClass }
        };
        TrackGrid.Children.Add(dummyPanel);
        Grid.SetColumn(dummyPanel, columnIndex);
    }
}