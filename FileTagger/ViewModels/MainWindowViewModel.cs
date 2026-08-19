using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FileTagger.Services;
using Microsoft.Extensions.DependencyInjection;
using ATL;
using ATL.Logging;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileTagger.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public const string UNCHANGED_FIELD = "< keep >";
    /// <summary>
    /// Gets a collection of <see cref="ATL.Track"/>
    /// </summary>
    public ObservableCollection<TrackViewModel> Tracks { get; } = [];

    public ObservableCollection<TrackViewModel> SelectedTracks { get; } = [];

    public ObservableCollection<string> TitleOptions { get; } = [];
    [ObservableProperty]
    private string? _titleText;

    public ObservableCollection<string> AlbumOptions { get; } = [];
    [ObservableProperty]
    private string? _albumText;

    [ObservableProperty]
    private bool _hasSelectedTracks;

    public void SelectionChanged()
    {
        TitleOptions.Clear();
        AlbumOptions.Clear();
        HasSelectedTracks = SelectedTracks.Count > 0;

        if (SelectedTracks.Count <= 0) return;

        TitleOptions.Add(UNCHANGED_FIELD);
        AlbumOptions.Add(UNCHANGED_FIELD);
        bool allSameTitle = true;
        bool allSameAlbum = true;
        string previousTitle = SelectedTracks[0].Title;
        string previousAlbum = SelectedTracks[0].Album;
        foreach (TrackViewModel track in SelectedTracks)
        {
            if (track.Title != previousTitle)
            {
                allSameTitle = false;
            }
            if (track.Album != previousAlbum)
            {
                allSameAlbum = false;
            }
            TitleOptions.Add(track.Title);
            AlbumOptions.Add(track.Album);
        }
        if (SelectedTracks.Count == 1)
        {
            TitleText = SelectedTracks[0].Title;
            AlbumText = SelectedTracks[0].Album;
        }
        else if(SelectedTracks.Count > 1)
        {
            TitleText = allSameTitle ? SelectedTracks[0].Title : UNCHANGED_FIELD;
            AlbumText = allSameAlbum ? SelectedTracks[0].Album : UNCHANGED_FIELD;
        }
    }

    public void FieldChanged()
    {
        if (TitleText != UNCHANGED_FIELD)
        {
            foreach (TrackViewModel track in SelectedTracks)
            {
                track.Title = TitleText;
            }
        }
        if (AlbumText != UNCHANGED_FIELD)
        {
            foreach (TrackViewModel track in SelectedTracks)
            {
                track.Album = AlbumText;
            }
        }
    }

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

            Tracks.Clear();
            SelectedTracks.Clear();
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