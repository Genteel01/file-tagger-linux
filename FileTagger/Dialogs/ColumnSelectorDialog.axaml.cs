using Avalonia.Controls;
using Avalonia.Input;
using FileTagger.Extensions;

namespace FileTagger.Dialogs;

public partial class ColumnSelectorDialog : Window
{
    public ColumnSelectorDialog()
    {
        InitializeComponent();
        DragDrop.SetAllowDrop(ColumnControl, true);
    }

    private void ListItemTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Panel panel) return;

        if (panel.GetFirstSibling<CheckBox>() is { } checkBox)
        {
            e.Handled = true;
            checkBox.IsChecked = !checkBox.IsChecked;
        }
    }
}