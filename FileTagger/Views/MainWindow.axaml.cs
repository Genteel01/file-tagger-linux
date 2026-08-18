using System;
using Avalonia.Controls;
using System.Diagnostics;
using Avalonia;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using FileTagger.ViewModels;

namespace FileTagger.Views;

public partial class MainWindow : Window
{

    private TextBox? _lastSelectedTextBox;

    private bool _leftControlHeld;
    private bool _rightControlHeld;

    private void Root_OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.LeftCtrl:
                _leftControlHeld = true;
                break;
            case Key.RightCtrl:
                _rightControlHeld = true;
                break;
            case Key.C:
                if (_leftControlHeld || _rightControlHeld)
                {
                    _lastSelectedTextBox?.Copy();
                }
                break;
        }
    }

    private void Root_OnKeyUp (object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.LeftCtrl:
                _leftControlHeld = false;
                break;
            case Key.RightCtrl:
                _rightControlHeld = false;
                break;
        }
    }

    public MainWindow()
    {
        InitializeComponent();

        TitleField.TextFilter = (search, item) =>
        {
            if (search == MainWindowViewModel.UNCHANGED_FIELD || item == MainWindowViewModel.UNCHANGED_FIELD || string.IsNullOrWhiteSpace(search))
            {
                return true;
            }

            return item?.Contains(search) ?? false;
        };
        AlbumField.TextFilter = (search, item) =>
        {
            if (search == MainWindowViewModel.UNCHANGED_FIELD || item == MainWindowViewModel.UNCHANGED_FIELD || string.IsNullOrWhiteSpace(search))
            {
                return true;
            }

            return item?.Contains(search) ?? false;
        };
    }

    private void OnListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectionChanged(SidePanel);
        }
    }

    /// <summary>
    /// When a text box is focused, treat it like selecting the row in the listBox
    /// </summary>
    private void TextBoxFocused(object? sender, FocusChangedEventArgs e)
    {
        //TODO control click to select multiple is broken
        if (sender is TextBox textBox)
        {
            _lastSelectedTextBox?.ClearSelection();
            _lastSelectedTextBox = textBox;
            ListBoxItem? listBoxItem = textBox.FindAncestorOfType<ListBoxItem>();
            ListBox? listBox = listBoxItem?.FindAncestorOfType<ListBox>();
            //Pretty sure all the ?s means that listBoxItem can't be null, so we're suppressing the warning with !
            listBox?.UpdateSelectionFromEvent(listBoxItem!, e);
            //Prevent focus on read only fields
            if (textBox.IsReadOnly)
            {
                listBoxItem?.Focus();
            }
        }
    }

    /// <summary>
    /// When a text box is tapped, clear the selection, to prevent selections when shift-clicking to select multiple rows
    /// </summary>
    private void TextBoxTapped(object? sender, TappedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (!textBox.IsFocused || textBox.IsReadOnly)
            {
                textBox.ClearSelection();
            }
        }
    }

    /// <summary>
    /// When a text box is double-tapped, allow editing and assign focus
    /// </summary>
    private void TextBoxDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.IsReadOnly = false;
            if (!textBox.IsFocused || textBox.IsReadOnly)
            {
                textBox.ClearSelection();
            }
            textBox.Focus();
        }
    }

    /// <summary>
    /// When a text box loses focus, make it read-only again
    /// </summary>
    private void TextBoxFocusLost(object? sender, FocusChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.IsReadOnly = true;
        }
    }

    private void ScrollViewerFocusLost(object? sender, FocusChangedEventArgs e)
    {
        _lastSelectedTextBox?.ClearSelection();
    }

    private void ValidateNumberOnlyField(object? sender, FocusChangedEventArgs e)
    {
        TextBoxFocusLost(sender, e);
        // Allow only digits
        if (sender is TextBox textBox)
        {
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
            Debug.Print("Printing Logical Children _________________________________________________________________________________________________________________");
            PrintLogicalChildren(box);
            Debug.Print("Printing Visual Children _________________________________________________________________________________________________________________");
            PrintVisualChildren(box);
        }
    }

    private void PrintLogicalChildren(ILogical root)
    {
        Debug.Print(root.ToString());

        foreach (var child in root.GetLogicalChildren())
        {
            if (child is TextBlock textBlock)
            {
                Debug.Print(child.ToString() + ": " + textBlock.Text);
            }
            else
            {
                Debug.Print(child.ToString());
            }
            PrintLogicalChildren(child);
        }
    }

    private void PrintVisualChildren(Visual root)
    {
        Debug.Print(root.ToString());
        foreach (var child in root.GetVisualChildren())
        {
            if (child is TextBlock textBlock)
            {
                Debug.Print(child.ToString() + ": " + textBlock.Text);
            }
            else
            {
                Debug.Print(child.ToString());
            }
            PrintVisualChildren(child);
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