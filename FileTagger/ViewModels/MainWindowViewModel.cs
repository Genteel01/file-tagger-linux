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

    public ObservableCollection<string> YearOptions { get; } = [];
    private ObservableCollection<int?> IntYearOptions { get; } = [];
    [ObservableProperty]
    private string _yearText = "";

    public ObservableCollection<string> TrackNumberOptions { get; } = [];
    private ObservableCollection<int?> IntTrackNumberOptions { get; } = [];
    [ObservableProperty]
    private string _trackNumberText = "";

    public ObservableCollection<string> GenreOptions { get; } = [];
    [ObservableProperty]
    private string _genreText = "";

    public ObservableCollection<string> CommentOptions { get; } = [];
    [ObservableProperty]
    private string _commentText = "";

    public ObservableCollection<string> AlbumArtistOptions { get; } = [];
    [ObservableProperty]
    private string _albumArtistText = "";

    public ObservableCollection<string> ComposerOptions { get; } = [];
    [ObservableProperty]
    private string _composerText = "";

    public ObservableCollection<string> DiscNumberOptions { get; } = [];
    private ObservableCollection<int?> IntDiscNumberOptions { get; } = [];
    [ObservableProperty]
    private string _discNumberText = "";

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
        IntYearOptions.Clear();
        YearOptions.Clear();
        IntTrackNumberOptions.Clear();
        TrackNumberOptions.Clear();
        GenreOptions.Clear();
        CommentOptions.Clear();
        AlbumArtistOptions.Clear();
        ComposerOptions.Clear();
        IntDiscNumberOptions.Clear();
        DiscNumberOptions.Clear();
        TitleText = "";
        AlbumText = "";
        ArtistText = "";
        YearText = "";
        TrackNumberText = "";
        GenreText = "";
        CommentText = "";
        AlbumArtistText = "";
        ComposerText = "";
        DiscNumberText = "";

        HasSelectedTracks = SelectedTracks.Count > 0;
        if (!HasSelectedTracks) return;

        foreach (TrackViewModel track in SelectedTracks)
        {
            if(track.Title != "" && !TitleOptions.Contains(track.Title)) TitleOptions.Add(track.Title);
            if(track.Album != "" && !AlbumOptions.Contains(track.Album)) AlbumOptions.Add(track.Album);
            if(track.Artist != "" && !ArtistOptions.Contains(track.Artist)) ArtistOptions.Add(track.Artist);
            if(!IntYearOptions.Contains(track.Year)) IntYearOptions.Add(track.Year);
            if(!IntTrackNumberOptions.Contains(track.TrackNumber)) IntTrackNumberOptions.Add(track.TrackNumber);
            if(track.Genre != "" && !GenreOptions.Contains(track.Genre)) GenreOptions.Add(track.Genre);
            if(track.Comment != "" && !CommentOptions.Contains(track.Comment)) CommentOptions.Add(track.Comment);
            if(track.AlbumArtist != "" && !AlbumArtistOptions.Contains(track.AlbumArtist)) AlbumArtistOptions.Add(track.AlbumArtist);
            if(track.Composer != "" && !ComposerOptions.Contains(track.Composer)) ComposerOptions.Add(track.Composer);
            if(!IntDiscNumberOptions.Contains(track.DiscNumber)) IntDiscNumberOptions.Add(track.DiscNumber);
        }
        if (SelectedTracks.Count == 1)
        {
            TitleText = SelectedTracks[0].Title;
            AlbumText = SelectedTracks[0].Album;
            ArtistText = SelectedTracks[0].Artist;
            YearText = SelectedTracks[0].Year.ToString() ?? "";
            TrackNumberText = SelectedTracks[0].TrackNumber.ToString() ?? "";
            GenreText = SelectedTracks[0].Genre;
            CommentText = SelectedTracks[0].Comment;
            AlbumArtistText = SelectedTracks[0].AlbumArtist;
            ComposerText = SelectedTracks[0].Composer;
            DiscNumberText = SelectedTracks[0].DiscNumber.ToString() ?? "";
        }
        else
        {
            TitleText = TitleOptions.All(title => title == SelectedTracks[0].Title) ? SelectedTracks[0].Title : UnchangedField;
            AlbumText = AlbumOptions.All(album => album == SelectedTracks[0].Album) ? SelectedTracks[0].Album : UnchangedField;
            ArtistText = ArtistOptions.All(artist => artist == SelectedTracks[0].Artist) ? SelectedTracks[0].Artist : UnchangedField;
            YearText = IntYearOptions.All(year => year == SelectedTracks[0].Year) ? SelectedTracks[0].Year.ToString() ?? "" : UnchangedField;
            TrackNumberText = IntTrackNumberOptions.All(trackNumber => trackNumber == SelectedTracks[0].TrackNumber) ? SelectedTracks[0].TrackNumber.ToString() ?? "" : UnchangedField;
            GenreText = GenreOptions.All(genre => genre == SelectedTracks[0].Genre) ? SelectedTracks[0].Genre : UnchangedField;
            CommentText = CommentOptions.All(comment => comment == SelectedTracks[0].Comment) ? SelectedTracks[0].Comment : UnchangedField;
            AlbumArtistText = AlbumArtistOptions.All(albumArtist => albumArtist == SelectedTracks[0].AlbumArtist) ? SelectedTracks[0].AlbumArtist : UnchangedField;
            ComposerText = ComposerOptions.All(composer => composer == SelectedTracks[0].Composer) ? SelectedTracks[0].Composer : UnchangedField;
            DiscNumberText = IntDiscNumberOptions.All(discNumber => discNumber == SelectedTracks[0].DiscNumber) ? SelectedTracks[0].DiscNumber.ToString() ?? "" : UnchangedField;
        }

        TitleOptions.Insert(0, UnchangedField);
        AlbumOptions.Insert(0, UnchangedField);
        ArtistOptions.Insert(0, UnchangedField);
        foreach (int? intYearOption in IntYearOptions)
        {
            if(intYearOption != null) YearOptions.Add(intYearOption.ToString()!);
        }
        YearOptions.Insert(0, UnchangedField);
        foreach (int? intTrackNumberOption in IntTrackNumberOptions)
        {
            if(intTrackNumberOption != null) TrackNumberOptions.Add(intTrackNumberOption.ToString()!);
        }
        TrackNumberOptions.Insert(0, UnchangedField);
        GenreOptions.Insert(0, UnchangedField);
        CommentOptions.Insert(0, UnchangedField);
        AlbumArtistOptions.Insert(0, UnchangedField);
        ComposerOptions.Insert(0, UnchangedField);
        foreach (int? intDiscNumberOption in IntDiscNumberOptions)
        {
            if(intDiscNumberOption != null) DiscNumberOptions.Add(intDiscNumberOption.ToString()!);
        }
        DiscNumberOptions.Insert(0, UnchangedField);
    }

    public void FieldChanged()
    {
        foreach (TrackViewModel track in SelectedTracks)
        {
            if (TitleText != UnchangedField) track.Title = TitleText;
            if (AlbumText != UnchangedField) track.Album = AlbumText;
            if (ArtistText != UnchangedField) track.Artist = ArtistText;
            if (YearText != UnchangedField)
            {
                bool parsed = int.TryParse(YearText, out int year);
                if (parsed) track.Year = year;
                else track.Year = null;
            }
            if (TrackNumberText != UnchangedField)
            {
                bool parsed = int.TryParse(TrackNumberText, out int trackNumber);
                if (parsed) track.TrackNumber = trackNumber;
                else track.TrackNumber = null;
            }
            if (GenreText != UnchangedField) track.Genre = GenreText;
            if (CommentText != UnchangedField) track.Comment = CommentText;
            if (AlbumArtistText != UnchangedField) track.AlbumArtist = AlbumArtistText;
            if (ComposerText != UnchangedField) track.Composer = ComposerText;
            if (DiscNumberText != UnchangedField)
            {
                bool parsed = int.TryParse(DiscNumberText, out int discNumber);
                if (parsed) track.DiscNumber = discNumber;
                else track.DiscNumber = null;
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


            (IReadOnlyList<IStorageFile>, bool) files = await filesService.OpenFilesRecursivelyAsync();
            if (files.Item2) return;

            Tracks.Clear();
            SelectedTracks.Clear();
            SelectionChanged();
            foreach (IStorageFile file in files.Item1)
            {
                string fileName = file.Name.ToLower();
                //TODO do this checking against ATL's supported types
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