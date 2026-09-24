using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FileTagger.ViewModels;

public partial class AutoNumberViewModel(Window dialog, List<TrackViewModel> tracks) : ViewModelBase
{
    public int? FirstValue
    {
        get
        {
            if (int.TryParse(FirstValueString, out int result)) return result;
            return null;
        }
        set => FirstValueString = value.ToString() ?? "";
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    public partial string FirstValueString { get; set; } = "1";

    public int? DiscNumber
    {
        get
        {
            if (int.TryParse(DiscNumberString, out int result)) return result;
            return null;
        }
        set => DiscNumberString = value.ToString() ?? "";
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    public partial string DiscNumberString { get; set; } = "1";

    [ObservableProperty] public partial bool SetDiscNumber { get; set; }
    [ObservableProperty] public partial bool ChangeDiscOnFolderChange { get; set; } = true;

    private bool CanConfirm => FirstValue != null && (!SetDiscNumber || DiscNumber != null);

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm()
    {
        string? previousDirectory = null;
        int? newDiscNumber = DiscNumber;
        foreach (TrackViewModel track in tracks)
        {
            track.TrackNumber = FirstValue++;
            if(SetDiscNumber)
            {
                if (ChangeDiscOnFolderChange)
                {
                    string? currentDirectory = Path.GetDirectoryName(track.Path);
                    if(previousDirectory != null && currentDirectory != previousDirectory) newDiscNumber++;
                    previousDirectory = currentDirectory;
                }
                track.DiscNumber = newDiscNumber;
            }
        }

        dialog.Close();
    }

    [RelayCommand]
    private void Cancel() => dialog.Close();
}