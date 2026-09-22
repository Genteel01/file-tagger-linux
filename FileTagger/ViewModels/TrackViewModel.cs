using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ATL;
using Commons;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileTagger.ViewModels;

/// <summary>
/// This is a ViewModel which represents a <see cref="ATL.Track"/>
/// </summary>
public partial class TrackViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the path
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets or sets the title
    /// </summary>
    [ObservableProperty]
    public partial string Title { get; set; }

    /// <summary>
    /// Gets or sets the album
    /// </summary>
    [ObservableProperty]
    public partial string Album { get; set; }

    /// <summary>
    /// Gets or sets the artist
    /// </summary>
    [ObservableProperty]
    public partial string Artist { get; set; }

    /// <summary>
    /// Gets or sets the track number
    /// </summary>
    [ObservableProperty]
    public partial int? TrackNumber { get; set; }

    /// <summary>
    /// Gets or sets the disc number
    /// </summary>
    [ObservableProperty]
    public partial int? DiscNumber { get; set; }

    /// <summary>
    /// Gets or sets the year
    /// </summary>
    [ObservableProperty]
    public partial int? Year { get; set; }

    /// <summary>
    /// Gets or sets the genre
    /// </summary>
    [ObservableProperty]
    public partial string Genre { get; set; }

    /// <summary>
    /// Gets or sets the album artist
    /// </summary>
    [ObservableProperty]
    public partial string AlbumArtist { get; set; }

    /// <summary>
    /// Gets or sets the composer
    /// </summary>
    [ObservableProperty]
    public partial string Composer { get; set; }

    /// <summary>
    /// Gets or sets the comment
    /// </summary>
    [ObservableProperty]
    public partial string Comment { get; set; }

    /// <summary>
    /// The Track that this ViewModel represents
    /// </summary>
    private readonly Track _originalTrack;

    private bool _isCoverSet = false;
    /// <summary>
    /// The Track's images. Loads them on first access because it's a slow operation and is memory intensive.
    /// We don't want to do it load them all at once, because it would increase load times, and we don't want to load
    /// them unless we actually need them, because it is a waste of memory.
    /// </summary>
    public ObservableCollection<PictureInfo> EmbeddedPictures
    {
        get
        {
            if (!_isCoverSet)
            {
                field = GetOriginalTrackImages();
                field.CollectionChanged += (_, _) =>
                {
                    for (int i = 0; i < EmbeddedPictures.Count; i++)
                    {
                        EmbeddedPictures[i].Position = i + 1;
                    }
                    Changed = true;
                };
                _isCoverSet = true;
            }
            return field;
        }
    } = [];

    /// <summary>
    /// Gets or sets whether the track has changed
    /// </summary>
    [ObservableProperty]
    public partial bool Changed { get; set; }

    private readonly bool _finishedSetup;

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
        _originalTrack = track;
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
    /// Gets the original track's embedded pictures
    /// </summary>
    /// <returns></returns>
    private ObservableCollection<PictureInfo> GetOriginalTrackImages()
    {
        return [.. _originalTrack.EmbeddedPictures.Where(pic => pic.NativeFormat != ImageFormat.Unsupported)];
    }

    /// <summary>
    /// Changes the description of the Embedded Picture at the given index
    /// </summary>
    public void ChangePictureDescription(string newDescription, int index)
    {
        string oldDescription = EmbeddedPictures[index].Description;
        EmbeddedPictures[index].Description = newDescription;
        if (oldDescription != newDescription) Changed = true;
    }

    /// <summary>
    /// Gets a Track of this ViewModel
    /// </summary>
    /// <returns>The Track</returns>
    private Track GetTrack()
    {
        Track thisTrack = _originalTrack;
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
        if (_isCoverSet)
        {
            thisTrack.EmbeddedPictures.Clear();
            foreach (PictureInfo picture in EmbeddedPictures)
            {
                thisTrack.EmbeddedPictures.Add(picture);
            }
        }
        return thisTrack;
    }


    /// <summary>
    /// Saves this track
    /// </summary>
    public void SaveTrackChanges()
    {
        if (Changed)
        {
            GetTrack().Save();
            Changed = false;
        }
    }
}