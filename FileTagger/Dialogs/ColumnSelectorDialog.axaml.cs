using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace FileTagger.Dialogs;

public partial class ColumnSelectorDialog : Window
{
    public ColumnSelectorDialog()
    {
        InitializeComponent();
        DragDrop.SetAllowDrop(ColumnControl, true);
    }
}