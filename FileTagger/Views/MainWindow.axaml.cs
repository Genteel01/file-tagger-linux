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