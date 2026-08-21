using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using FileTagger.ViewModels;

namespace FileTagger.Views;

public partial class TrackList : UserControl
{
    private TextBox? _lastSelectedTextBox;

    private void Root_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.C && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            _lastSelectedTextBox?.Copy();
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
            _lastSelectedTextBox?.ClearSelection();
            TrackListBox.UnselectAll();
        }
        else if (e.NewFocusedElement is Control c && sender is Control s && !s.IsLogicalAncestorOf(c))
        {
            _lastSelectedTextBox?.ClearSelection();
        }
    }

    /// <summary>
    /// When a text box is focused, treat it like selecting the row in the listBox
    /// </summary>
    private void TextBoxFocused(object? sender, FocusChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        _lastSelectedTextBox?.ClearSelection();
        _lastSelectedTextBox = textBox;
        ListBoxItem? listBoxItem = textBox.FindAncestorOfType<ListBoxItem>();
        ListBox? listBox = listBoxItem?.FindAncestorOfType<ListBox>();
        //Pretty sure all the ?s means that listBoxItem can't be null, so we're suppressing the warning with !
        listBox?.UpdateSelectionFromEvent(listBoxItem!, e);
        //ListBox doesn't do multi-select with Ctrl when selection is changed by Focus, so we have to handle that ourselves
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            Grid? grid = textBox.FindAncestorOfType<Grid>();
            if (grid != null)
            {
                if (DataContext is MainWindowViewModel vm)
                {
                    vm.ToggleSelect(grid.Name ?? "");
                }
            }

        }
        //Prevent focus on read only fields
        if (textBox.IsReadOnly)
        {
            listBoxItem?.Focus();
        }
    }

    /// <summary>
    /// When a text box is tapped, clear the selection, to prevent selections when shift-clicking to select multiple rows
    /// </summary>
    private void TextBoxTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        if (!textBox.IsFocused || textBox.IsReadOnly)
        {
            textBox.ClearSelection();
        }
    }

    /// <summary>
    /// When a text box is double-tapped, allow editing and assign focus
    /// </summary>
    private void TextBoxDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        textBox.IsReadOnly = false;
        if (!textBox.IsFocused || textBox.IsReadOnly)
        {
            textBox.ClearSelection();
        }
        textBox.Focus();
    }

    private void ValidateNumberOnlyField(object? sender, FocusChangedEventArgs e)
    {
        TextBoxFocusLost(sender, e);
        // Allow only digits
        if (sender is not TextBox textBox) return;

        string newText = textBox.Text ?? "";
        for (int i = newText.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(newText[i]))
            {
                newText = newText.Remove(i, 1);
            }
        }
        textBox.Text = newText;
    }

    /// <summary>
    /// When a text box loses focus, make it read-only again
    /// </summary>
    private void TextBoxFocusLost(object? sender, FocusChangedEventArgs e)
    {
        if (sender is TextBox textBox && e.NewFocusedElement is not MenuItem)
        {
            textBox.IsReadOnly = true;
        }
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectionChanged();
        }
    }
}