using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ATL;
using ATL.AudioData;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileTagger.Services;

namespace FileTagger.ViewModels;

public partial class EditPanelViewModel: ViewModelBase, IRecipient<MainWindowViewModel.SelectedItemsMessage>
{
    /// <summary>
    /// Value for edit fields that we don't want to change
    /// </summary>
    public const string UnchangedField = "< keep >";

    /// <summary>
    /// All the tracks that are currently selected
    /// </summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasSelectedTracks))]
    private List<TrackViewModel> _selectedTracks = [];

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
    /// Whether SelectedTracks is not empty
    /// </summary>
    public bool HasSelectedTracks => SelectedTracks.Count > 0;

    /// <summary>
    /// <see cref="IFileService"/> received through Dependency Injection used for opening file dialog
    /// </summary>
    private readonly IFileService _fileService;

    /// <summary>
    /// Function to get the <see cref="TopLevel"/>, which we use to get the <see cref="IClipboard"/> for Cut/Copy/Paste of images
    /// </summary>
    private readonly Func<TopLevel?> _getTopLevel;

    /// <summary>
    /// <see cref="IImageService"/> received through Dependency Injection used for handling images
    /// </summary>
    private readonly IImageService _imageService;

    /// <summary>
    /// Array of properties of TrackViewModel that we want to be editable
    /// </summary>
    private readonly PropertyInfo[] _trackProperties;

    public EditPanelViewModel(IFileService fileService, IImageService imageService, Func<TopLevel?> getTopLevel)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _imageService =  imageService ?? throw new ArgumentNullException(nameof(imageService));
        _getTopLevel = getTopLevel;
        IsActive = true;

        //Select properties that are writable, and are either string or int?
        _trackProperties = [.. typeof(TrackViewModel).GetProperties().Where(property => property.CanWrite &&
            (property.PropertyType == typeof(string) ||  property.PropertyType == typeof(int?)) )];
        FieldTexts = SetUpFieldTexts();
        FieldOptions = SetUpFieldOptions();
    }

    #if DEBUG
    /// <summary>
    /// Default Constructor for design time
    /// </summary>
    public EditPanelViewModel()
    {
        _trackProperties = [];
        _fileService = new FileService(() => null);
        _imageService = new ImageService();
    }
    #endif

    /// <summary>
    /// Receives the event containing the selected tracks
    /// </summary>
    public void Receive(MainWindowViewModel.SelectedItemsMessage message)
    {
        SelectedTracks =  message.Tracks;
        SelectionChanged();
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
    /// Handle setting up the edit field texts and options, based on the currently selected tracks
    /// </summary>
    private void SelectionChanged()
    {
        ChooseDisplayedImage();
        Dictionary<string, string> newFieldTexts = SetUpFieldTexts();
        Dictionary<string, List<object?>> newFieldOptions = SetUpFieldOptions();

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
                    .All(property => Equals(property, propertyInfo.GetValue(SelectedTracks[0])));
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
    /// Predicate for <see cref="IEnumerable&lt;PictureInfo>"/> LINQ expressions to check
    /// if <see cref="PictureInfo.PicType"/> matches <see cref="SelectedPictureType"/>
    /// <![CDATA[ IEnumerable<PictureInfo> ]]>
    /// </summary>
    private bool MatchesSelectedPicType(PictureInfo pic)
    {
        return pic.PicType == SelectedPictureType;
    }
    /// <summary>
    /// Opens the file picker for the user to select new images,
    /// then replaces the cover images of the currently selected type, for the currently selected tracks
    /// </summary>
    [RelayCommand]
    private async Task ReplaceCoverImage(CancellationToken token)
    {
        List<PictureInfo> images = await SelectImageFiles(SelectedPictureType);

        if(images.Count == 0) return;
        foreach (TrackViewModel track in SelectedTracks)
        {
            track.EmbeddedPictures.RemoveAll(MatchesSelectedPicType);
            track.EmbeddedPictures.AddRange(images);
            track.Changed = true;
        }
        ChooseDisplayedImage();
    }

    /// <summary>
    /// Opens the file picker for the user to select new images,
    /// then adds them to the cover images of the currently selected type, for the currently selected tracks
    /// </summary>
    [RelayCommand]
    private async Task AddCoverImages(CancellationToken token)
    {
        List<PictureInfo> images = await SelectImageFiles(SelectedPictureType);

        if(images.Count == 0) return;
        foreach (TrackViewModel track in SelectedTracks)
        {
            track.EmbeddedPictures.AddRange(images);
            for (int i = 0; i < track.EmbeddedPictures.Count; i++)
            {
                track.EmbeddedPictures[i].Position = i + 1;
            }
            track.Changed = true;
        }

        ChooseDisplayedImage();
        DisplayedImageIndex = SelectedTrackImages.Count - 1;
    }

    /// <summary>
    /// Opens the file picker to select image files and returns a list of <see cref="PictureInfo"/> for every image
    /// </summary>
    private async Task<List<PictureInfo>> SelectImageFiles(PictureInfo.PIC_TYPE pictureType)
    {
        ErrorMessages?.Clear();
        try
        {
            if (_fileService is null) throw new NullReferenceException("Missing File Service instance.");

            IReadOnlyList<IStorageFile> files = await _fileService.OpenImageFiles();

            List<PictureInfo> images = [];

            for (int i = 0; i < files.Count; i++)
            {
                IStorageFile file = files[i];
                await using Stream stream = await file.OpenReadAsync();
                PictureInfo picInfo = PictureInfo.fromBinaryData(stream, (int)stream.Length,
                    pictureType, MetaDataIOFactory.TagType.ANY, 0, i + 1);
                picInfo.ComputePicHash();
                images.Add(picInfo);
            }

            return images;
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
            return [];
        }
    }

    /// <summary>
    /// Removes the visible cover image from the selected tracks, or remove all cover images of the selected type
    /// from all selected tracks if there are different covers for the selected tracks
    /// </summary>
    [RelayCommand]
    private void RemoveCoverImage()
    {
        if (IsShowingTrackImages)
        {
            RemoveCurrentlyDisplayedImages(SelectedTracks);
        }
        else
        {
            foreach (TrackViewModel track in SelectedTracks)
            {
                int oldCount = track.EmbeddedPictures.Count;
                track.EmbeddedPictures.RemoveAll(MatchesSelectedPicType);
                if(track.EmbeddedPictures.Count != oldCount) track.Changed = true;
            }
        }
        ChooseDisplayedImage();
    }

    /// <summary>
    /// Removes the currently displayed image from the given tracks
    /// </summary>
    private void RemoveCurrentlyDisplayedImages(List<TrackViewModel> tracks)
    {
        foreach (TrackViewModel track in tracks)
        {
            RemoveCurrentlyDisplayedImage(track);
        }
    }

    /// <summary>
    /// Removes the currently displayed image from the given track
    /// Returns the index of the removed image in the given track's EmbeddedPictures
    /// </summary>
    private int RemoveCurrentlyDisplayedImage(TrackViewModel track)
    {
        PictureInfo displayedImage = SelectedTrackImages[DisplayedImageIndex];
        int index = track.EmbeddedPictures.FindIndex(pic =>
            _imageService.ArePicturesIdentical(displayedImage, pic) && pic.Equals(displayedImage));
        track.EmbeddedPictures.RemoveAt(index);
        track.Changed = true;
        return index;
    }

    [RelayCommand]
    private async Task CutCoverImage()
    {
        await CopyCoverImage();
        RemoveCurrentlyDisplayedImages(SelectedTracks);
    }

    [RelayCommand]
    private async Task CopyCoverImage()
    {
        IClipboard? clipboard = _getTopLevel.Invoke()?.Clipboard;
        if (clipboard == null) return;
        Bitmap bitmap = CurrentDisplayedImage;
        DataTransfer data = new DataTransfer();
        data.Add(DataTransferItem.Create(DataFormat.Bitmap, bitmap));
        await clipboard.SetDataAsync(data);
    }

    /// <summary>
    /// Chooses which image to display in the edit panel
    /// </summary>
    private void ChooseDisplayedImage()
    {
        DisplayedImageIndex = 0;
        SelectedTrackImages = [];
        if (!HasSelectedTracks) return;

        List<PictureInfo> referencePics = SelectedTracks[0].EmbeddedPictures.Where(MatchesSelectedPicType).ToList();
        if (referencePics.Count == 0) return;
        //If there's only one track selected, display its images
        if (SelectedTracks.Count == 1)
        {
            SelectedTrackImages = referencePics;
            return;
        }
        //If there is more than one track selected, display its images if they are the same across the entire selection
        List<List<PictureInfo>> picsPerTrack = [];
        //Skip the first track, since its images are in referencePics
        foreach (TrackViewModel track in SelectedTracks.Skip(1))
        {
            List<PictureInfo> relevantPics = track.EmbeddedPictures.Where(MatchesSelectedPicType).ToList();
            if (relevantPics.Count != referencePics.Count) return;
            picsPerTrack.Add(relevantPics);
        }

        for (int i = 0; i < referencePics.Count; i++)
        {
            bool allTracksMatch = picsPerTrack.All(pics => _imageService.ArePicturesIdentical(referencePics[i], pics[i]));
            if(!allTracksMatch) return;
        }
        SelectedTrackImages = referencePics;
    }

    /// <summary>
    /// Gets a <see cref="Bitmap"/> of the given <see cref="PictureInfo"/>
    /// </summary>
    private Bitmap GetAndCacheBitmap(PictureInfo picInfo)
    {
        _cachedImages.TryGetValue(picInfo.PictureHash, out Bitmap? bitmap);
        if (bitmap == null)
        {
            bitmap = new Bitmap(new MemoryStream(picInfo.PictureData));
            _cachedImages[picInfo.PictureHash] = bitmap;
        }
        return bitmap;
    }

    /// <summary>
    /// Choose the correct image to display when <see cref="SelectedPictureType"/> changes
    /// </summary>
    // ReSharper disable once UnusedParameterInPartialMethod
    partial void OnSelectedPictureTypeChanged(PictureInfo.PIC_TYPE value)
    {
        ChooseDisplayedImage();
    }

    /// <summary>
    /// Show the next image for the current selected type, looping around
    /// </summary>
    [RelayCommand]
    private void NextImage()
    {
        DisplayedImageIndex  = (DisplayedImageIndex + 1) % SelectedTrackImages.Count;
    }

    /// <summary>
    /// Show the previous image for the current selected type, looping around
    /// </summary>
    [RelayCommand]
    private void PreviousImage()
    {
        int newIndex = DisplayedImageIndex - 1;
        if (newIndex < 0) newIndex = SelectedTrackImages.Count - 1;
        DisplayedImageIndex = newIndex;
    }

    /// <summary>
    /// List of images for the <see cref="SelectedTracks"/> for the <see cref="SelectedPictureType"/>.
    /// Will be empty if selected tracks have different images
    /// </summary>
    [NotifyPropertyChangedFor(nameof(ShowImageNavigationButtons))]
    [NotifyPropertyChangedFor(nameof(CurrentDisplayedImage))]
    [NotifyPropertyChangedFor(nameof(IsShowingTrackImages))]
    [ObservableProperty]
    private List<PictureInfo> _selectedTrackImages = [];

    /// <summary>
    /// The index representing which entry in <see cref="SelectedTrackImages"/> to display
    /// </summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CurrentDisplayedImage))]
    private int _displayedImageIndex = 0;

    /// <summary>
    /// Bitmap of the entry in <see cref="SelectedTrackImages"/> to display based on <see cref="DisplayedImageIndex"/>,
    /// showing <see cref="_imageService"/>.GetDefaultImage() if SelectedTrackImages is empty.
    /// Updates when SelectedTrackImages or DisplayedImageIndex change
    /// </summary>
    public Bitmap CurrentDisplayedImage => IsShowingTrackImages ? GetAndCacheBitmap(SelectedTrackImages[DisplayedImageIndex]) : _imageService.GetDefaultImage();

    /// <summary>
    /// Whether to show the navigation buttons for moving between images. Updates when <see cref="SelectedTrackImages"/> changes
    /// </summary>
    public bool ShowImageNavigationButtons => SelectedTrackImages.Count > 1;

    /// <summary>
    /// Whether we are showing the images of the selected tracks, or the default image
    /// </summary>
    public bool IsShowingTrackImages => SelectedTrackImages.Count > 0;

    /// <summary>
    /// List of <see cref="PictureInfo.PIC_TYPE"/> enum values as strings, for populating a selection dropdown
    /// </summary>
    public PictureInfo.PIC_TYPE[] PictureTypes { get; } = Enum.GetValues<PictureInfo.PIC_TYPE>();

    /// <summary>
    /// String of the selected <see cref="PictureInfo.PIC_TYPE"/> from the dropdown
    /// </summary>
    [ObservableProperty]
    private PictureInfo.PIC_TYPE _selectedPictureType = PictureInfo.PIC_TYPE.Front;

    /// <summary>
    /// Dictionary of Bitmaps mapped to the corresponding <see cref="PictureInfo.PictureHash"/>,
    /// so we don't have to re-decode the same image multiple times
    /// </summary>
    private readonly Dictionary<uint, Bitmap> _cachedImages = new Dictionary<uint, Bitmap>();
}