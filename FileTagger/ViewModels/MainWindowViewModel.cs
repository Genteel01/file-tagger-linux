using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FileTagger.Services;
using Microsoft.Extensions.DependencyInjection;
using ATL;
using ATL.AudioData;
using ATL.Logging;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileTagger.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Value for edit fields that we don't want to change
    /// </summary>
    public const string UnchangedField = "< keep >";

    /// <summary>
    /// Default image to display with either no or multiple tracks selected
    /// </summary>
    private readonly Bitmap _defaultImage =
        new Bitmap(AssetLoader.Open(new Uri("avares://FileTagger/Assets/placeholder.png", UriKind.Absolute)));

    /// <summary>
    /// All the tracks that have been loaded in
    /// </summary>
    public ObservableCollection<TrackViewModel> Tracks { get; } = [];

    /// <summary>
    /// All the tracks that are currently selected
    /// </summary>
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
    [ObservableProperty]
    private string _yearText = "";

    public ObservableCollection<string> TrackNumberOptions { get; } = [];
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
    [ObservableProperty]
    private string _discNumberText = "";

    [ObservableProperty]
    private bool _hasSelectedTracks;

    public MainWindowViewModel()
    {
        IEnumerable<string> picTypes = Enum.GetNames<PictureInfo.PIC_TYPE>();
        PictureTypes = new ObservableCollection<string>(picTypes);

        SelectedPictureType = Enum.GetName(PictureInfo.PIC_TYPE.Front)!;
        _selectedImages.Add(_defaultImage);
        SelectedImageIndex = 0;
        CurrentDisplayedImage = _selectedImages.First();
    }
    /// <summary>
    /// Toggle selecting a track with the given path
    /// </summary>
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

    /// <summary>
    /// Handle setting up the edit field texts and options, based on the currently selected tracks
    /// </summary>
    public void SelectionChanged()
    {
        ChooseDisplayedImage();
        TitleOptions.Clear();
        AlbumOptions.Clear();
        ArtistOptions.Clear();
        List<int?> newYearOptions = [];
        YearOptions.Clear();
        List<int?> newTrackNumberOptions = [];
        TrackNumberOptions.Clear();
        GenreOptions.Clear();
        CommentOptions.Clear();
        AlbumArtistOptions.Clear();
        ComposerOptions.Clear();
        List<int?> newDiscNumberOptions = [];
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
            if(!TitleOptions.Contains(track.Title)) TitleOptions.Add(track.Title);
            if(!AlbumOptions.Contains(track.Album)) AlbumOptions.Add(track.Album);
            if(!ArtistOptions.Contains(track.Artist)) ArtistOptions.Add(track.Artist);
            if(!newYearOptions.Contains(track.Year)) newYearOptions.Add(track.Year);
            if(!newTrackNumberOptions.Contains(track.TrackNumber)) newTrackNumberOptions.Add(track.TrackNumber);
            if(!GenreOptions.Contains(track.Genre)) GenreOptions.Add(track.Genre);
            if(!CommentOptions.Contains(track.Comment)) CommentOptions.Add(track.Comment);
            if(!AlbumArtistOptions.Contains(track.AlbumArtist)) AlbumArtistOptions.Add(track.AlbumArtist);
            if(!ComposerOptions.Contains(track.Composer)) ComposerOptions.Add(track.Composer);
            if(!newDiscNumberOptions.Contains(track.DiscNumber)) newDiscNumberOptions.Add(track.DiscNumber);
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
            YearText = newYearOptions.All(year => year == SelectedTracks[0].Year) ? SelectedTracks[0].Year.ToString() ?? "" : UnchangedField;
            TrackNumberText = newTrackNumberOptions.All(trackNumber => trackNumber == SelectedTracks[0].TrackNumber) ? SelectedTracks[0].TrackNumber.ToString() ?? "" : UnchangedField;
            GenreText = GenreOptions.All(genre => genre == SelectedTracks[0].Genre) ? SelectedTracks[0].Genre : UnchangedField;
            CommentText = CommentOptions.All(comment => comment == SelectedTracks[0].Comment) ? SelectedTracks[0].Comment : UnchangedField;
            AlbumArtistText = AlbumArtistOptions.All(albumArtist => albumArtist == SelectedTracks[0].AlbumArtist) ? SelectedTracks[0].AlbumArtist : UnchangedField;
            ComposerText = ComposerOptions.All(composer => composer == SelectedTracks[0].Composer) ? SelectedTracks[0].Composer : UnchangedField;
            DiscNumberText = newDiscNumberOptions.All(discNumber => discNumber == SelectedTracks[0].DiscNumber) ? SelectedTracks[0].DiscNumber.ToString() ?? "" : UnchangedField;
        }

        TitleOptions.Insert(0, UnchangedField);
        TitleOptions.Remove("");
        AlbumOptions.Insert(0, UnchangedField);
        AlbumOptions.Remove("");
        ArtistOptions.Insert(0, UnchangedField);
        ArtistOptions.Remove("");
        foreach (int? intYearOption in newYearOptions)
        {
            if(intYearOption != null) YearOptions.Add(intYearOption.ToString()!);
        }
        YearOptions.Insert(0, UnchangedField);
        foreach (int? intTrackNumberOption in newTrackNumberOptions)
        {
            if(intTrackNumberOption != null) TrackNumberOptions.Add(intTrackNumberOption.ToString()!);
        }
        TrackNumberOptions.Insert(0, UnchangedField);
        GenreOptions.Insert(0, UnchangedField);
        GenreOptions.Remove("");
        CommentOptions.Insert(0, UnchangedField);
        CommentOptions.Remove("");
        AlbumArtistOptions.Insert(0, UnchangedField);
        AlbumArtistOptions.Remove("");
        ComposerOptions.Insert(0, UnchangedField);
        ComposerOptions.Remove("");
        foreach (int? intDiscNumberOption in newDiscNumberOptions)
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

    /// <summary>
    /// Saves all the tracks to disk
    /// </summary>
    [RelayCommand]
    private void SaveMusicFiles()
    {
        foreach (TrackViewModel track in Tracks)
        {
            track.SaveTrackChanges();
        }
    }

    /// <summary>
    /// Opens the file picker to choose a directory, then loads all music files in that directory as <see cref="TrackViewModel"/>
    /// </summary>
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

    /// <summary>
    /// Opens the file picker for the user to select new images,
    /// then replaces the cover images of the currently selected type, for the currently selected tracks
    /// </summary>
    [RelayCommand]
    private async Task ReplaceCoverImage(CancellationToken token)
    {
        ErrorMessages?.Clear();
        try
        {
            IFileService? filesService = App.Current?.Services?.GetService<IFileService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            IReadOnlyList<IStorageFile> files = await filesService.OpenImageFiles();

            List<(Bitmap, PictureInfo)> images = [];

            PictureInfo.PIC_TYPE pictureType = Enum.Parse<PictureInfo.PIC_TYPE>(SelectedPictureType);
            foreach (IStorageFile file in files)
            {
                Stream stream = await file.OpenReadAsync();
                Bitmap bitmap = new Bitmap(stream);
                stream = await file.OpenReadAsync();
                PictureInfo picInfo = PictureInfo.fromBinaryData(stream, (int)stream.Length,
                    pictureType, MetaDataIOFactory.TagType.ANY, 0);
                images.Add((bitmap, picInfo));
            }

            if(images.Count == 0) return;
            foreach (TrackViewModel track in SelectedTracks)
            {
                track.EmbeddedPictures.RemoveAll(pic => pic.Item2.PicType == pictureType);
                track.EmbeddedPictures.AddRange(images);
            }
            ChooseDisplayedImage();
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
            Debug.Fail(e.Message);
        }
    }

    /// <summary>
    /// Chooses which image to display in the edit panel
    /// </summary>
    private void ChooseDisplayedImage()
    {
        _selectedImages.Clear();
        _selectedImages.Add(_defaultImage);
        SelectedImageIndex = 0;
        CurrentDisplayedImage = _selectedImages.First();
        ShowImageNavigationButtons = false;
        if (SelectedTracks.Count == 0) return;

        PictureInfo.PIC_TYPE pictureType = Enum.Parse<PictureInfo.PIC_TYPE>(SelectedPictureType);

        //If there's only one track selected, display all its images
        if (SelectedTracks.Count == 1)
        {
            List<(Bitmap, PictureInfo)> validPics = SelectedTracks.First().EmbeddedPictures.Where(pic => pic.Item2.PicType == pictureType).ToList();
            if (validPics.Count == 0) return;

            _selectedImages.Clear();
            foreach ((Bitmap, PictureInfo) pic in validPics)
            {
                _selectedImages.Add(pic.Item1);
            }
            CurrentDisplayedImage = _selectedImages.First();
            ShowImageNavigationButtons = _selectedImages.Count > 1;
        }
        //If there is more than one track selected, display its images if they are the same across the entire selection
        else if (SelectedTracks.All(track => track.EmbeddedPictures.Any(pic => pic.Item2.PicType == pictureType)))
        {
            //Get a list of the pics of the correct type for each selected track
            List<List<(Bitmap, PictureInfo)>> validPicsPerTrack = [];
            foreach (TrackViewModel track in SelectedTracks)
            {
                List<(Bitmap, PictureInfo)> trackPics =
                    [.. track.EmbeddedPictures.Where(pic => pic.Item2.PicType == pictureType)];
                validPicsPerTrack.Add(trackPics);
            }
            //If each selected track doesn't have the same number of pics, we already know they don't match and can move on
            int firstCount = validPicsPerTrack.First().Count;
            bool matchingSizes = validPicsPerTrack.All(trackPics => trackPics.Count == firstCount);
            if (!matchingSizes) return;

            //Check whether each pic matches all the other pics of the same index
            bool picsMatch = true;
            for (int i = 0; i < firstCount; i++)
            {
                (Bitmap, PictureInfo) firstTrackPic = validPicsPerTrack.First()[i];
                if (!validPicsPerTrack.All(trackPics =>
                        ArePicturesIdentical(firstTrackPic.Item2.PictureData, trackPics[i].Item2.PictureData)))
                {
                    picsMatch = false;
                    break;
                }
            }

            if (!picsMatch) return;

            _selectedImages.Clear();
            //If all pics match, display them all
            foreach ((Bitmap, PictureInfo) pic in validPicsPerTrack.First())
            {
                _selectedImages.Add(pic.Item1);
            }
            CurrentDisplayedImage = _selectedImages.First();
            ShowImageNavigationButtons = _selectedImages.Count > 1;
        }
    }

    /// <summary>
    /// Determines whether two pictures are identical by examining their Byte data
    /// </summary>
    /// <returns></returns>
    private static bool ArePicturesIdentical(byte[] pic1, byte[] pic2)
    {
        if(pic1.Length != pic2.Length) return false;

        for (int i = 0; i < pic1.Length; i++)
        {
            if (pic1[i] != pic2[i]) return false;
        }
        return true;
    }

    /// <summary>
    /// Choose the correct image to display when the selected type changes
    /// </summary>
    partial void OnSelectedPictureTypeChanged(string value)
    {
        ChooseDisplayedImage();
    }

    /// <summary>
    /// Show the next image for the current selected type, looping around
    /// </summary>
    [RelayCommand]
    private void NextImage()
    {
        SelectedImageIndex += 1;
        SelectedImageIndex %= _selectedImages.Count;
        CurrentDisplayedImage = _selectedImages[SelectedImageIndex];
    }

    /// <summary>
    /// Show the previous image for the current selected type, looping around
    /// </summary>
    [RelayCommand]
    private void PreviousImage()
    {
        SelectedImageIndex -= 1;
        if(SelectedImageIndex < 0) SelectedImageIndex = _selectedImages.Count - 1;
        CurrentDisplayedImage = _selectedImages[SelectedImageIndex];
    }

    private List<Bitmap> _selectedImages = [];
    [ObservableProperty] private int _selectedImageIndex;
    [ObservableProperty] private Bitmap _currentDisplayedImage;
    [ObservableProperty] private bool _showImageNavigationButtons;

    public ObservableCollection<string> PictureTypes { get; }
    [ObservableProperty] private string _selectedPictureType;
}