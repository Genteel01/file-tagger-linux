using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace FileTagger.Dialogs;

public partial class PicDescriptionDialog : Window
{
    public PicDescriptionDialog()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        InputBox.Focus();
        InputBox.CaretIndex = InputBox.Text?.Length ?? 0;
    }

    private void DialogKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SaveButton.Command?.Execute(null);
        }
        else if (e.Key == Key.Escape)
        {
            CancelButton.Command?.Execute(null);
        }
    }
}