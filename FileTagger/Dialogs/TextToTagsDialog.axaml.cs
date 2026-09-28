using Avalonia.Controls;
using FileTagger.ViewModels;

namespace FileTagger.Dialogs;

public partial class TextToTagsDialog : Window
{
    public TextToTagsDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// When you select an item in the dropdown, add it to the format field, focus it, and clear the selection
    /// </summary>
    private void NewPropertySelected(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox) return;
        if (listBox.SelectedItem is not string s) return;
        if (s == "") return;
        if (DataContext is TextToTagViewModel vm)
        {
            vm.AddProperty(s);
        }
        listBox.SelectedItem = null;
        FormatField.Focus();
        FormatField.CaretIndex = FormatField.Text?.Length ?? 0;
        InsertButton.Flyout?.Hide();
    }
}