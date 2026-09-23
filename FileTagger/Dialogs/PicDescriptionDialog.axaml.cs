using Avalonia.Controls;
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
}