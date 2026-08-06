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
    /// Creates a new TrackViewModel for the given <see cref="ATL.Track"/>
    /// </summary>
    /// <param name="track">The Track to load</param>
    public TrackViewModel(Track track)
    {
        Path = track.Path;
        Title = track.Title;
        Album = track.Album;
        Artist = track.Artist;
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