using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using FileTagger.Extensions;
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
            string searchName = textBox.Name?.Replace("Box", "Block") ?? "";

            TextBlock? siblingBlock = textBox.GetFirstSibling<TextBlock>(box => box.Name == searchName);

            if (siblingBlock == null) return;

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

    /// <summary>
    /// When a <see cref="TextBlock"/> is double-tapped, switch to the associated <see cref="TextBox"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBlockDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not TextBlock textBlock) return;

        string searchName = textBlock.Name?.Replace("Block", "Box") ?? "";
        TextBox? siblingBox = textBlock.GetFirstSibling<TextBox>(box => box.Name == searchName);

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
}