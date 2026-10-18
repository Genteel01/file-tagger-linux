using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;

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
        if (e.Pointer.Captured is not Panel panel) return;
        if (panel.Children.FirstOrDefault(child => child is CheckBox) is CheckBox checkBox)
        {
            checkBox.IsChecked = !checkBox.IsChecked;
        }
    }
}