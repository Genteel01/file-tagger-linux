using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ATL;
using CommunityToolkit.Mvvm.ComponentModel;
using FileTagger.Extensions;

namespace FileTagger.ViewModels;

/// <summary>
/// This is a ViewModel which represents a <see cref="ATL.Track"/>
/// </summary>
public partial class TrackViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the path
    /// </summary>
    public string Path => Directory + FileName;

    /// <summary>
    /// Gets the path
    /// </summary>
    public string Directory { get; }

    /// <summary>
    /// Gets the path
    /// </summary>
    public string FileName { get; }

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

    /// <summary>
    /// In a list of tracks, IsChangeStart = true when Changed = true and the previous item on the list has Changed = false
    /// </summary>
    [ObservableProperty] public partial bool IsChangeStart { get; set; } = false;
    /// <summary>
    /// In a list of tracks, IsChangeEnd = true when Changed = true and the next item on the list has Changed = false
    /// </summary>
    [ObservableProperty] public partial bool IsChangeEnd { get; set; } = false;

    /// <summary>
    /// Whether this track's tags are being cut
    /// </summary>
    [ObservableProperty] public partial bool IsCutting { get; set; } = false;

    private bool _finishedSetup = false;

    private static readonly string[] IgnoreChangeProperties =
    [
        nameof(Changed), nameof(IsChangeStart), nameof(IsChangeEnd), nameof(IsCutting)
    ];

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (_finishedSetup && !IgnoreChangeProperties.Contains(e.PropertyName))
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
        string? directory = System.IO.Path.GetDirectoryName(track.Path);
        Directory = directory == null ? "" : directory + System.IO.Path.DirectorySeparatorChar;
        FileName = System.IO.Path.GetFileName(track.Path);
        SetUpViewModel();
    }

    /// <summary>
    /// Creates a new TrackViewModel as a copy of an existing one. Will only contain the editable tags of the track
    /// </summary>
    public TrackViewModel(TrackViewModel original)
    {
        Directory = "";
        FileName = "";
        _originalTrack = new Track();
        original.CopyTo(this);
    }

    private void SetUpViewModel()
    {
        _finishedSetup = false;
        Title = _originalTrack.Title;
        Album = _originalTrack.Album;
        Artist = _originalTrack.Artist;
        TrackNumber = _originalTrack.TrackNumber;
        DiscNumber = _originalTrack.DiscNumber;
        Year = _originalTrack.Year;
        Genre = _originalTrack.Genre;
        AlbumArtist = _originalTrack.AlbumArtist;
        Composer = _originalTrack.Composer;
        Comment = _originalTrack.Comment;

        if(_isCoverSet)
        {
            EmbeddedPictures.Clear();
            EmbeddedPictures.AddRange(GetOriginalTrackImages());
        }
        Changed = false;
        IsChangeStart = false;
        IsChangeEnd = false;
        _finishedSetup = true;
    }

    /// <summary>
    /// Gets the original track's embedded pictures
    /// </summary>
    /// <returns></returns>
    private ObservableCollection<PictureInfo> GetOriginalTrackImages()
    {
        return [.. _originalTrack.EmbeddedPictures.Select(pic => new PictureInfo(pic))];
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
    /// Changes the type of the Embedded Picture at the given index
    /// </summary>
    public void ChangePictureType(PictureInfo.PIC_TYPE newType, int index)
    {
        PictureInfo.PIC_TYPE oldType = EmbeddedPictures[index].PicType;
        EmbeddedPictures[index].PicType = newType;
        if (oldType != newType) Changed = true;
    }

    /// <summary>
    /// Gets a Track of this ViewModel
    /// </summary>
    /// <returns>The Track</returns>
    private Track GetTrack()
    {
        _originalTrack.Title = Title;
        _originalTrack.Album = Album;
        _originalTrack.Artist = Artist;
        _originalTrack.TrackNumber = TrackNumber;
        _originalTrack.DiscNumber = DiscNumber;
        _originalTrack.Year = Year;
        _originalTrack.Genre = Genre;
        _originalTrack.AlbumArtist = AlbumArtist;
        _originalTrack.Composer = Composer;
        _originalTrack.Comment = Comment;
        if (_isCoverSet)
        {
            _originalTrack.EmbeddedPictures.Clear();
            foreach (PictureInfo picture in EmbeddedPictures)
            {
                _originalTrack.EmbeddedPictures.Add(picture);
            }
        }
        return _originalTrack;
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
            IsChangeStart = false;
            IsChangeEnd = false;
        }
    }

    public void RevertChanges()
    {
        SetUpViewModel();
    }

    /// <summary>
    /// Copies the editable Properties of this TrackViewModel to another one
    /// </summary>
    public void CopyTo(TrackViewModel copy)
    {
        copy.Title = Title;
        copy.Album = Album;
        copy.Artist = Artist;
        copy.TrackNumber = TrackNumber;
        copy.DiscNumber = DiscNumber;
        copy.Year = Year;
        copy.Genre = Genre;
        copy.AlbumArtist = AlbumArtist;
        copy.Composer = Composer;
        copy.Comment = Comment;
        copy.EmbeddedPictures.Clear();
        foreach (PictureInfo picture in EmbeddedPictures)
        {
            copy.EmbeddedPictures.Add(new PictureInfo(picture));
        }
    }

    /// <summary>
    /// Clears all tags from this track
    /// </summary>
    public void ClearTags()
    {
        Title = "";
        Album = "";
        Artist = "";
        TrackNumber = null;
        DiscNumber = null;
        Year = null;
        Genre = "";
        AlbumArtist = "";
        Composer = "";
        Comment = "";
    }
}