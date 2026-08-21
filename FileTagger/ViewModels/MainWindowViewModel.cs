using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FileTagger.Services;
using Microsoft.Extensions.DependencyInjection;
using ATL;
using ATL.Logging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileTagger.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public const string UnchangedField = "< keep >";
    /// <summary>
    /// Gets a collection of <see cref="ATL.Track"/>
    /// </summary>
    public ObservableCollection<TrackViewModel> Tracks { get; } = [];

    public ObservableCollection<TrackViewModel> SelectedTracks { get; } = [];

    public ObservableCollection<string> TitleOptions { get; } = [];
    [ObservableProperty]
    private string _titleText = "";

    public ObservableCollection<string> AlbumOptions { get; } = [];
    [ObservableProperty]
    private string _albumText = "";

    public ObservableCollection<string> ArtistOptions { get; } = [];
    [ObservableProperty]
    private string _artistText = "";

    [ObservableProperty]
    private bool _hasSelectedTracks;

    public void ToggleSelect(string path)
    {

        IEnumerable<TrackViewModel> tracksWithPath = Tracks.Where(track => track.Path == path).ToList();
        IEnumerable<TrackViewModel> selectedTracksWithPath = SelectedTracks.Where(track => track.Path == path).ToList();
        if (selectedTracksWithPath.Any())
        {
            SelectedTracks.Remove(selectedTracksWithPath.First());
        }
        else if (tracksWithPath.Any())
        {
            SelectedTracks.Add(tracksWithPath.First());
        }
    }
    public void SelectionChanged()
    {
        TitleOptions.Clear();
        AlbumOptions.Clear();
        ArtistOptions.Clear();
        TitleText = "";
        AlbumText = "";
        ArtistText = "";
        HasSelectedTracks = SelectedTracks.Count > 0;

        if (!HasSelectedTracks) return;

        foreach (TrackViewModel track in SelectedTracks)
        {
            if(!TitleOptions.Contains(track.Title)) TitleOptions.Add(track.Title);
            if(!AlbumOptions.Contains(track.Album)) AlbumOptions.Add(track.Album);
            if(!ArtistOptions.Contains(track.Artist)) ArtistOptions.Add(track.Artist);
        }
        if (SelectedTracks.Count == 1)
        {
            TitleText = SelectedTracks[0].Title;
            AlbumText = SelectedTracks[0].Album;
            ArtistText = SelectedTracks[0].Artist;
        }
        else
        {
            TitleText = TitleOptions.All(title => title == SelectedTracks[0].Title) ? SelectedTracks[0].Title : UnchangedField;
            AlbumText = AlbumOptions.All(album => album == SelectedTracks[0].Album) ? SelectedTracks[0].Album : UnchangedField;
            ArtistText = ArtistOptions.All(artist => artist == SelectedTracks[0].Artist) ? SelectedTracks[0].Artist : UnchangedField;
        }

        TitleOptions.Insert(0, UnchangedField);
        AlbumOptions.Insert(0, UnchangedField);
        ArtistOptions.Insert(0, UnchangedField);
    }

    public void FieldChanged()
    {
        foreach (TrackViewModel track in SelectedTracks)
        {
            if (TitleText != UnchangedField) track.Title = TitleText;

            if (AlbumText != UnchangedField) track.Album = AlbumText;

            if (ArtistText != UnchangedField) track.Artist = ArtistText;

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
            SelectionChanged();
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