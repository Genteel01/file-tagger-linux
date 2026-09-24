using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FileTagger.Dialogs;

public partial class AutoNumberDialog : Window
{
    public AutoNumberDialog()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        InitialFocus.Focus();
        InitialFocus.CaretIndex = InitialFocus.Text?.Length ?? 0;
    }
}