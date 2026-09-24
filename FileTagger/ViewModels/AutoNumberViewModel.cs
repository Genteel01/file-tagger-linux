using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FileTagger.ViewModels;

public partial class AutoNumberViewModel(Window dialog, List<TrackViewModel> tracks) : ViewModelBase
{
    [ObservableProperty] public partial int FirstValue { get; set; } = 1;
    [ObservableProperty] public partial bool SetDiscNumber { get; set; }
    [ObservableProperty] public partial bool ChangeDiscOnFolderChange { get; set; } = true;
    [ObservableProperty] public partial int DiscNumber { get; set; } = 1;

    [RelayCommand]
    private void Confirm()
    {
        string? previousDirectory = null;
        int newDiscNumber = DiscNumber;
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