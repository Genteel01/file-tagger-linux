using Avalonia.Controls;
using System.Diagnostics;
using Avalonia.Input;
using Avalonia.VisualTree;
using FileTagger.ViewModels;

namespace FileTagger.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            Debug.Print(vm.SelectedTracks.Count.ToString());
        }
    }

    /// <summary>
    /// When a text box is focused, treat it like selecting the row in the listBox
    /// </summary>
    private void TextBoxFocused(object? sender, FocusChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            ListBoxItem? listBoxItem = textBox.FindAncestorOfType<ListBoxItem>();
            ListBox? listBox = listBoxItem?.FindAncestorOfType<ListBox>();
            //Pretty sure all the ?s means that listBoxItem can't be null, so we're suppressing the warning
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
            textBox.ClearSelection();
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
            textBox.ClearSelection();
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
            textBox.ClearSelection();
        }
    }
}