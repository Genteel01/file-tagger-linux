using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileTagger.Services;
using Microsoft.Extensions.DependencyInjection;
using ATL;
using ATL.Logging;

namespace FileTagger.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _fileText;

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
            var filesService = App.Current?.Services?.GetService<IFileService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            var files = await filesService.OpenFilesRecursivelyAsync();

            FileText = "";
            foreach (IStorageFile file in files)
            {
                string fileName = file.Name.ToLower();
                if (fileName.EndsWith(".mp3") ||  fileName.EndsWith(".wav") || fileName.EndsWith(".flac"))
                {
                    Track track = new Track(file.Path.LocalPath);
                    Tracks.Add(new TrackViewModel(track));

                    FileText += "Absolute Path: " + file.Path.AbsolutePath + "\n ";
                    FileText += "Local Path: " + file.Path.LocalPath + "\n ";
                    FileText += "Title: " + track.Title + "\n ";
                    FileText += "Album: " + track.Album + "\n ";
                    FileText += "Artist: " + track.Artist + "\n ";
                    FileText += "Track Number: " + track.TrackNumber + "\n ";
                    FileText += "Album Artist: " + track.AlbumArtist + "\n ";
                    FileText += "Year: " + track.Year + "\n ";
                    FileText += "Genre: " + track.Genre + "\n ";
                    foreach (string key in track.AdditionalFields.Keys)
                    {
                        FileText += key + ": " + track.AdditionalFields[key] + "\n ";
                    }
                }
            }
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }
    }
}