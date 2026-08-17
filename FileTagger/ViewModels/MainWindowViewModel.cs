using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FileTagger.Services;
using Microsoft.Extensions.DependencyInjection;
using ATL;
using ATL.Logging;

namespace FileTagger.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Gets a collection of <see cref="ATL.Track"/>
    /// </summary>
    public ObservableCollection<TrackViewModel> Tracks { get; } = new ObservableCollection<TrackViewModel>();

    [RelayCommand]
    private void SaveMusicFiles()
    {
        foreach (TrackViewModel track in Tracks)
        {
            track.SaveTrackChanges();
        }
    }

    [RelayCommand]
    private async Task OpenMusicFiles(CancellationToken token)
    {
        ConsoleLogger log = new ConsoleLogger();
        ErrorMessages?.Clear();
        try
        {
            IFileService? filesService = App.Current?.Services?.GetService<IFileService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            IReadOnlyList<IStorageFile> files = await filesService.OpenFilesRecursivelyAsync();

            foreach (IStorageFile file in files)
            {
                string fileName = file.Name.ToLower();
                if (fileName.EndsWith(".mp3") ||  fileName.EndsWith(".wav") || fileName.EndsWith(".flac"))
                {
                    Track track = new Track(file.Path.LocalPath);
                    Tracks.Add(new TrackViewModel(track));
                }
            }
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }
    }
}