using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
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

            IEnumerable<ILogical> siblings = textBox.GetLogicalSiblings();
            TextBlock? siblingBlock = null;
            foreach (ILogical sibling in siblings)
            {
                if (sibling is TextBlock block && block.Name == searchName)
                {
                    siblingBlock = block;
                }
            }

            if (siblingBlock != null)
            {
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
        IEnumerable<ILogical> siblings = textBlock.GetLogicalSiblings();
        TextBox? siblingBox = null;
        foreach (ILogical sibling in siblings)
        {
            if (sibling is TextBox box && box.Name == searchName)
            {
                siblingBox = box;
            }
        }

        if (siblingBox != null)
        {
            textBlock.IsVisible = false;
            textBlock.IsEnabled = false;
            siblingBox.IsVisible = true;
            siblingBox.IsEnabled = true;
            siblingBox.SelectAll();
            siblingBox.Focus();
        }
    }
}