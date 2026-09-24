using System.Collections.Generic;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FileTagger.ViewModels;

public partial class AutoNumberViewModel : ViewModelBase
{
    [ObservableProperty] public partial int FirstValue { get; set; }

    private readonly Window _dialog;

    private readonly List<TrackViewModel> _tracks;

    public AutoNumberViewModel(Window dialog, List<TrackViewModel> tracks, int initialValue)
    {
        _dialog = dialog;
        _tracks = tracks;
        FirstValue = initialValue;
    }

    [RelayCommand]
    private void Confirm()
    {
        foreach (TrackViewModel track in _tracks)
        {
            track.TrackNumber = FirstValue++;
        }

        _dialog.Close();
    }

    [RelayCommand]
    private void Cancel() => _dialog.Close();
}