using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
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

    /// <summary>
    /// The value in the edit box for each field
    /// </summary>
    [ObservableProperty]
    private Dictionary<string, string> _fieldTexts = new Dictionary<string, string>();

    /// <summary>
    /// The options in the edit box dropdown for each field
    /// </summary>
    [ObservableProperty]
    private Dictionary<string, List<object?>> _fieldOptions = new Dictionary<string, List<object?>>();

    /// <summary>
    /// Whether SelectedTracks.Count > 0
    /// </summary>
    [ObservableProperty]
    private bool _hasSelectedTracks;

    /// <summary>
    /// Array of properties of TrackViewModel that we want to be editable
    /// </summary>
    private readonly PropertyInfo[] _trackProperties;
    public MainWindowViewModel()
    {
        IEnumerable<string> picTypes = Enum.GetNames<PictureInfo.PIC_TYPE>();
        PictureTypes = new ObservableCollection<string>(picTypes);

        SelectedPictureType = Enum.GetName(PictureInfo.PIC_TYPE.Front)!;
        _selectedImages.Add(_defaultImage);
        SelectedImageIndex = 0;
        CurrentDisplayedImage = _selectedImages.First();

        //Select properties that are writable, and are either string or int?
        _trackProperties = [.. typeof(TrackViewModel).GetProperties().Where(property => property.CanWrite &&
            (property.PropertyType == typeof(string) ||  property.PropertyType == typeof(int?)) )];
        FieldTexts = SetUpFieldTexts();
        FieldOptions = SetUpFieldOptions();
    }

    /// <summary>
    /// Get a Dictionary of each field's text with a default value
    /// </summary>
    private Dictionary<string, string> SetUpFieldTexts()
    {
        Dictionary<string, string> newFieldTexts = new Dictionary<string, string>();
        foreach (PropertyInfo propertyInfo in _trackProperties)
        {
            newFieldTexts[propertyInfo.Name] = "";
        }
        return newFieldTexts;
    }

    /// <summary>
    /// Get a Dictionary of each field's options with default empty lists
    /// </summary>
    private Dictionary<string, List<object?>> SetUpFieldOptions()
    {
        Dictionary<string, List<object?>> newFieldOptions = new Dictionary<string, List<object?>>();
        foreach (PropertyInfo propertyInfo in _trackProperties)
        {
            newFieldOptions[propertyInfo.Name] = [];
        }
        return newFieldOptions;
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
        Dictionary<string, string> newFieldTexts = SetUpFieldTexts();
        Dictionary<string, List<object?>> newFieldOptions = SetUpFieldOptions();

        HasSelectedTracks = SelectedTracks.Count > 0;
        if (!HasSelectedTracks)
        {
            FieldTexts = newFieldTexts;
            FieldOptions = newFieldOptions;
            return;
        }

        //For each selected track, add its value of each field to the options for that field
        foreach (TrackViewModel track in SelectedTracks)
        {
            foreach (PropertyInfo propertyInfo in _trackProperties)
            {
                if(!newFieldOptions[propertyInfo.Name].Contains(propertyInfo.GetValue(track)))
                    newFieldOptions[propertyInfo.Name].Add(propertyInfo.GetValue(track));
            }
        }
        //If we only have one selected track, set each field text to the value of that field, or blank if null
        if (SelectedTracks.Count == 1)
        {
            foreach (PropertyInfo propertyInfo in _trackProperties)
            {
                newFieldTexts[propertyInfo.Name] = propertyInfo.GetValue(SelectedTracks[0])?.ToString() ?? "";
            }
        }
        //If we have more than one selected track, set each field text to the value of that field if it is the same on every track
        //otherwise set it to UnchangedField
        else
        {
            foreach (PropertyInfo propertyInfo in _trackProperties)
            {
                bool allTracksMatch = newFieldOptions[propertyInfo.Name]
                    .All(property => property == propertyInfo.GetValue(SelectedTracks[0]));
                newFieldTexts[propertyInfo.Name] = allTracksMatch ? propertyInfo.GetValue(SelectedTracks[0])?.ToString() ?? "" : UnchangedField;
            }
        }

        //Add UnchangedField as an option for each field, and remove blank options
        foreach (PropertyInfo propertyInfo in _trackProperties)
        {
            newFieldOptions[propertyInfo.Name].Insert(0, UnchangedField);
            newFieldOptions[propertyInfo.Name].Remove("");
            newFieldOptions[propertyInfo.Name].Remove(null);
        }
        FieldTexts = newFieldTexts;
        FieldOptions = newFieldOptions;
    }

    /// <summary>
    /// Stores the values in the edit fields to each selected track
    /// </summary>
    public void StoreFieldChanges()
    {
        foreach (TrackViewModel track in SelectedTracks)
        {
            foreach (PropertyInfo propertyInfo in _trackProperties)
            {
                if (FieldTexts[propertyInfo.Name] == UnchangedField) continue;

                if (propertyInfo.PropertyType == typeof(string))
                {
                    propertyInfo.SetValue(track, FieldTexts[propertyInfo.Name]);
                }
                else if (propertyInfo.PropertyType == typeof(int?))
                {
                    bool parsed = int.TryParse(FieldTexts[propertyInfo.Name], out int parsedInt);
                    if (parsed) propertyInfo.SetValue(track, parsedInt);
                    else propertyInfo.SetValue(track, null);
                }
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
                track.Changed = true;
            }
            ChooseDisplayedImage();
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
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