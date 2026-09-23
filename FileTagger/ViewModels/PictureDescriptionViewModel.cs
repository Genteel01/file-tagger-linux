using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FileTagger.ViewModels;

public partial class PictureDescriptionViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string PicDescription { get; set; }

    private readonly Window _dialog;

    public PictureDescriptionViewModel(Window dialog, string initialDescription)
    {
        _dialog = dialog;
        PicDescription = initialDescription;
    }

    [RelayCommand]
    private void Close() => _dialog.Close(PicDescription);

    [RelayCommand]
    private void Cancel() => _dialog.Close(null);
}