using System.ComponentModel;
using ATL;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileTagger.ViewModels;

/// <summary>
/// This is a ViewModel which represents a <see cref="ATL.Track"/>
/// </summary>
public partial class TrackViewModel : ViewModelBase
{
    /// <summary>
    /// Gets or sets the path
    /// </summary>
    [ObservableProperty]
    private string _path;

    /// <summary>
    /// Gets or sets the title
    /// </summary>
    [ObservableProperty]
    private string _title;

    /// <summary>
    /// Gets or sets the album
    /// </summary>
    [ObservableProperty]
    private string? _album;

    /// <summary>
    /// Gets or sets the artist
    /// </summary>
    [ObservableProperty]
    private string? _artist;

    /// <summary>
    /// Gets or sets the track number
    /// </summary>
    [ObservableProperty]
    private int? _trackNumber;

    /// <summary>
    /// Gets or sets the disc number
    /// </summary>
    [ObservableProperty]
    private int? _discNumber;

    /// <summary>
    /// Gets or sets the year
    /// </summary>
    [ObservableProperty]
    private int? _year;

    /// <summary>
    /// Gets or sets the genre
    /// </summary>
    [ObservableProperty]
    private string? _genre;

    /// <summary>
    /// Gets or sets the album artist
    /// </summary>
    [ObservableProperty]
    private string? _albumArtist;

    /// <summary>
    /// Gets or sets the composer
    /// </summary>
    [ObservableProperty]
    private string? _composer;

    /// <summary>
    /// Gets or sets the comment
    /// </summary>
    [ObservableProperty]
    private string? _comment;

    /// <summary>
    /// Gets or sets whether the track has changed
    /// </summary>
    [ObservableProperty]
    private bool _changed;

    private bool _finishedSetup;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (_finishedSetup && e.PropertyName != "Changed")
        {
            Changed = true;
        }
    }

    /// <summary>
    /// Creates a new TrackViewModel for the given <see cref="ATL.Track"/>
    /// </summary>
    /// <param name="track">The Track to load</param>
    public TrackViewModel(Track track)
    {
        Path = track.Path;
        Title = track.Title;
        Album = track.Album;
        Artist = track.Artist;
        TrackNumber = track.TrackNumber;
        DiscNumber = track.DiscNumber;
        Year = track.Year;
        Genre = track.Genre;
        AlbumArtist = track.AlbumArtist;
        Composer = track.Composer;
        Comment = track.Comment;
        _finishedSetup = true;
    }

    /// <summary>
    /// Gets a Track of this ViewModel
    /// </summary>
    /// <returns>The Track</returns>
    public Track GetTrack()
    {
        Track thisTrack = new Track(Path);
        thisTrack.Title = Title;
        thisTrack.Album = Album;
        thisTrack.Artist = Artist;
        thisTrack.TrackNumber = TrackNumber;
        thisTrack.DiscNumber = DiscNumber;
        thisTrack.Year = Year;
        thisTrack.Genre = Genre;
        thisTrack.AlbumArtist = AlbumArtist;
        thisTrack.Composer = Composer;
        thisTrack.Comment = Comment;
        return thisTrack;
    }


    /// <summary>
    /// Saves this track
    /// </summary>
    public void SaveTrackChanges()
    {
        GetTrack().Save();
    }
}