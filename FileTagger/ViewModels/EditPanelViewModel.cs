using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ATL;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileTagger.Assets.Statics;
using FileTagger.Extensions;
using FileTagger.Services;

namespace FileTagger.ViewModels;

public partial class EditPanelViewModel: ViewModelBase, IRecipient<MainWindowViewModel.SelectedItemsMessage>
{
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
    /// <see cref="IImageService"/> received through Dependency Injection used for handling images
    /// </summary>
    private readonly IImageService _imageService;

    /// <summary>
    /// Array of properties of TrackViewModel that we want to be editable
    /// </summary>
    private readonly PropertyInfo[] _trackProperties;

    public EditPanelViewModel(IFileService fileService, IImageService imageService)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _imageService =  imageService ?? throw new ArgumentNullException(nameof(imageService));
        IsActive = true;

        PictureTypes = Enum.GetValues<PictureInfo.PIC_TYPE>();
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
        PictureTypes = Enum.GetValues<PictureInfo.PIC_TYPE>();
    }
    #endif

    /// <summary>
    /// Receives the event containing the selected tracks
    /// </summary>
    public void Receive(MainWindowViewModel.SelectedItemsMessage message)
    {
        SelectedTracks = message.Tracks;
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
                newFieldTexts[propertyInfo.Name] = allTracksMatch ? propertyInfo.GetValue(SelectedTracks[0])?.ToString() ?? "" : Consts.UnchangedField;
            }
        }

        //Add UnchangedField as an option for each field, and remove blank options
        foreach (PropertyInfo propertyInfo in _trackProperties)
        {
            newFieldOptions[propertyInfo.Name].Insert(0, Consts.UnchangedField);
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
                if (FieldTexts[propertyInfo.Name] == Consts.UnchangedField) continue;

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
    /// </summary>
    private bool MatchesSelectedPicType(PictureInfo pic)
    {
        return pic.PicType == SelectedPictureType;
    }
    /// <summary>
    /// Opens the file picker for the user to select new images, then replaces the currently displayed image,
    /// or replaces all images of the current <see cref="SelectedPictureType"/> is we aren't showing individual images
    /// </summary>
    [RelayCommand]
    private async Task ReplaceCoverImage(CancellationToken token)
    {
        List<PictureInfo> images = await SelectImageFiles(SelectedPictureType);
        if(images.Count == 0) return;

        if (IsShowingTrackImages)
        {
            ReplaceDisplayedImage(SelectedTracks, images);
        }
        else
        {
            ReplaceAllImages(SelectedTracks, images);
        }
        ChooseDisplayedImage();
    }

    /// <summary>
    /// Replaces the currently displayed image on the given tracks, then adds the new images
    /// </summary>
    private void ReplaceDisplayedImage(List<TrackViewModel> tracks, List<PictureInfo> newImages)
    {
        foreach (TrackViewModel track in tracks)
        {
            int index = RemoveCurrentlyDisplayedImage(track);
            if (index == -1)
            {
                track.EmbeddedPictures.AddRange(newImages);
            }
            else
            {
                track.EmbeddedPictures.InsertRange(index, newImages);
            }
        }

    }

    /// <summary>
    /// Replaces the images on the given tracks with the new images
    /// </summary>
    private void ReplaceAllImages(List<TrackViewModel> tracks, List<PictureInfo> newImages)
    {
        foreach (TrackViewModel track in tracks)
        {
            track.EmbeddedPictures.RemoveAll(MatchesSelectedPicType);
            track.EmbeddedPictures.AddRange(newImages);
        }
    }

    /// <summary>
    /// Opens the file picker for the user to select new images,
    /// then adds them to the EmbeddedPictures of the currently selected type, for the currently selected tracks
    /// </summary>
    [RelayCommand]
    private async Task AddCoverImages(CancellationToken token)
    {
        List<PictureInfo> images = await SelectImageFiles(SelectedPictureType);

        if(images.Count == 0) return;
        AddImagesToTracks(SelectedTracks, images);

        ChooseDisplayedImage();
        DisplayedImageIndex = SelectedTrackImages.Count - 1;
    }

    /// <summary>
    /// Adds the given images to the given tracks
    /// </summary>
    private void AddImagesToTracks(List<TrackViewModel> tracks, List<PictureInfo> images)
    {
        foreach (TrackViewModel track in tracks)
        {
            track.EmbeddedPictures.AddRange(images);
        }
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

            foreach (IStorageFile file in files)
            {
                await using Stream stream = await file.OpenReadAsync();
                PictureInfo picInfo = _imageService.CreatePictureInfoFromStream(stream, pictureType);
                images.Add(picInfo);
            }

            return images;
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
            throw;
        }
    }

    /// <summary>
    /// Removes the visible picture from the selected tracks, or remove all pictures of the selected type
    /// from all selected tracks if there are different pictures for the selected tracks
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
                track.EmbeddedPictures.RemoveAll(MatchesSelectedPicType);
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
    /// Removes the currently displayed image from the given track.
    /// Returns the index of the removed image in the given track's EmbeddedPictures
    /// </summary>
    private int RemoveCurrentlyDisplayedImage(TrackViewModel track)
    {
        if (!IsShowingTrackImages) return -1;
        PictureInfo displayedImage = SelectedImage!;
        int index = track.EmbeddedPictures.FindIndex(pic => pic.TrueEqual(displayedImage));
        if (index != -1)
        {
            track.EmbeddedPictures.RemoveAt(index);
        }
        return index;
    }

    /// <summary>
    /// Whether we have a copied image
    /// </summary>
    public bool HasImageClipboardData => CopiedPic != null;

    /// <summary>
    /// Decides whether to show the display image as being cut.
    /// Is true if <see cref="TracksToCutFrom"/> contains all of <see cref="SelectedTracks"/> and the displayed image is <see cref="CopiedPic"/>
    /// </summary>
    public bool DisplayedImageIsBeingCut
    {
        get
        {
            if (!HasSelectedTracks || !IsShowingTrackImages) return false;
            if(CopiedPic == null) return false;
            if(TracksToCutFrom.Count < SelectedTracks.Count) return false;
            if (!CopiedPic.TrueEqual(SelectedImage!)) return false;
            return SelectedTracks.All(track => TracksToCutFrom.Any(cutTrack => cutTrack == track));
        }
    }

    /// <summary>
    /// List of tracks we are cutting images from
    /// </summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DisplayedImageIsBeingCut))]
    private List<TrackViewModel> _tracksToCutFrom = [];

    /// <summary>
    /// The currently copied image, if there is one
    /// </summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasImageClipboardData))]
    private PictureInfo? _copiedPic = null;

    [RelayCommand]
    private void CutCoverImage()
    {
        CopyCoverImage();
        TracksToCutFrom = [.. SelectedTracks];
    }

    /// <summary>
    /// Removes the images that were cut from the tracks they were cut from
    /// </summary>
    private void FinishCutting()
    {
        if (TracksToCutFrom.Count == 0) return;
        if (CopiedPic == null) return;
        foreach (TrackViewModel track in TracksToCutFrom)
        {
            track.EmbeddedPictures.RemoveAll(trackPic => trackPic.TrueEqual(CopiedPic));
        }
        TracksToCutFrom = [];
    }

    [RelayCommand]
    private void CopyCoverImage()
    {
        CopiedPic = null;
        CopiedPic = SelectedImage;
        TracksToCutFrom = [];
    }

    [RelayCommand]
    private void PasteCoverImageAsReplacement()
    {
        if(CopiedPic == null) return;
        FinishCutting();
        CopiedPic.PicType = SelectedPictureType;
        List<PictureInfo> images = [CopiedPic];
        if (IsShowingTrackImages)
        {
            ReplaceDisplayedImage(SelectedTracks, images);
        }
        else
        {
            ReplaceAllImages(SelectedTracks, images);
        }
        ChooseDisplayedImage();
    }

    [RelayCommand]
    private void PasteCoverImageAsNew()
    {
        if(CopiedPic == null) return;
        FinishCutting();
        CopiedPic.PicType = SelectedPictureType;
        List<PictureInfo> images = [CopiedPic];
        AddImagesToTracks(SelectedTracks, images);

        ChooseDisplayedImage();
        DisplayedImageIndex = SelectedTrackImages.Count - 1;
    }

    /// <summary>
    /// Chooses which image to display in the edit panel
    /// </summary>
    private void ChooseDisplayedImage()
    {
        int oldDisplayedIndex = DisplayedImageIndex;
        PictureInfo? oldDisplayedImage = SelectedImage;
        DisplayedImageIndex = 0;
        SelectedTrackImages = [];
        HasNoImages = true;
        if (!HasSelectedTracks) return;

        SelectedTrackImages = GetSelectedTrackImages();
        if (oldDisplayedImage != null && SelectedTrackImages.Count > oldDisplayedIndex)
        {
            if (SelectedTrackImages[oldDisplayedIndex].TrueEqual(oldDisplayedImage)) DisplayedImageIndex = oldDisplayedIndex;
        }
    }

    /// <summary>
    /// Gets the images from SelectedTracks, if all of them have the same images
    /// </summary>
    /// <returns></returns>
    private List<PictureInfo> GetSelectedTrackImages()
    {
        List<PictureInfo> referencePics = SelectedTracks[0].EmbeddedPictures.Where(MatchesSelectedPicType).ToList();
        if(referencePics.Count != 0) HasNoImages = false;
        //If there's only one track selected, display its images
        if (SelectedTracks.Count == 1) return referencePics;
        //If there is more than one track selected, display its images if they are the same across the entire selection
        List<List<PictureInfo>> picsPerTrack = [];
        //Skip the first track, since its images are in referencePics
        foreach (TrackViewModel track in SelectedTracks.Skip(1))
        {
            List<PictureInfo> relevantPics = track.EmbeddedPictures.Where(MatchesSelectedPicType).ToList();
            if (relevantPics.Count != 0)
            {
                HasNoImages = false;
            }
            if (relevantPics.Count != referencePics.Count) return [];
            picsPerTrack.Add(relevantPics);
        }

        for (int i = 0; i < referencePics.Count; i++)
        {
            bool allTracksMatch = picsPerTrack.All(pics => pics[i].PicturesEqual(referencePics[i]));
            if(!allTracksMatch) return [];
        }
        return referencePics;
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
    /// Switch to the next picture type, looping around
    /// </summary>
    [RelayCommand]
    private void NextPicType()
    {
        int currentIndex = PictureTypes.IndexOf(SelectedPictureType);
        if (currentIndex == -1) return;
        SelectedPictureType = PictureTypes[(currentIndex + 1) % PictureTypes.Length];
    }

    /// <summary>
    /// Switch to the previous picture type, looping around
    /// </summary>
    [RelayCommand]
    private void PreviousPicType()
    {
        int currentIndex = PictureTypes.IndexOf(SelectedPictureType);
        if (currentIndex == -1) return;
        int newIndex = currentIndex - 1;
        if (newIndex < 0) newIndex = PictureTypes.Length - 1;
        SelectedPictureType = PictureTypes[newIndex];
    }

    /// <summary>
    /// List of images for the <see cref="SelectedTracks"/> for the <see cref="SelectedPictureType"/>.
    /// Will be empty if selected tracks have different images
    /// </summary>
    [NotifyPropertyChangedFor(nameof(ShowImageNavigationButtons))]
    [NotifyPropertyChangedFor(nameof(CurrentDisplayedImage))]
    [NotifyPropertyChangedFor(nameof(IsShowingTrackImages))]
    [NotifyPropertyChangedFor(nameof(DisplayedImageIsBeingCut))]
    [NotifyPropertyChangedFor(nameof(PicCountString))]
    [ObservableProperty]
    private List<PictureInfo> _selectedTrackImages = [];

    /// <summary>
    /// The index representing which entry in <see cref="SelectedTrackImages"/> to display
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentDisplayedImage))]
    [NotifyPropertyChangedFor(nameof(DisplayedImageIsBeingCut))]
    [NotifyPropertyChangedFor(nameof(PicCountString))]
    private int _displayedImageIndex = 0;

    /// <summary>
    /// String to indicate how many pictures there are, and which one is being viewed
    /// </summary>
    public string PicCountString => $"{DisplayedImageIndex + 1}/{SelectedTrackImages.Count}";

    /// <summary>
    /// The PictureInfo of the image that is currently being displayed
    /// </summary>
    private PictureInfo? SelectedImage => IsShowingTrackImages ? SelectedTrackImages[DisplayedImageIndex] : null;

    /// <summary>
    /// Bitmap of <see cref="SelectedImage"/>, showing <see cref="_imageService"/>.GetDefaultImage() if SelectedTrackImages is empty.
    /// Updates when SelectedTrackImages or DisplayedImageIndex change
    /// </summary>
    public Bitmap? CurrentDisplayedImage => IsShowingTrackImages ? GetAndCacheBitmap(SelectedImage!) : null;

    /// <summary>
    /// Whether to show the navigation buttons for moving between images. Updates when <see cref="SelectedTrackImages"/> changes
    /// </summary>
    public bool ShowImageNavigationButtons => SelectedTrackImages.Count > 1;

    /// <summary>
    /// Whether we are showing the images of the selected tracks, or the default image
    /// </summary>
    public bool IsShowingTrackImages => SelectedTrackImages.Count > 0;

    /// <summary>
    /// Whether all SelectedTracks have no EmbeddedPictures
    /// </summary>
    [ObservableProperty]
    private bool _hasNoImages;

    /// <summary>
    /// List of <see cref="PictureInfo.PIC_TYPE"/> enum values for populating a selection dropdown
    /// </summary>
    public PictureInfo.PIC_TYPE[] PictureTypes { get; }

    /// <summary>
    /// The selected <see cref="PictureInfo.PIC_TYPE"/> from the dropdown
    /// </summary>
    [ObservableProperty]
    private PictureInfo.PIC_TYPE _selectedPictureType = PictureInfo.PIC_TYPE.Front;

    /// <summary>
    /// Dictionary of Bitmaps mapped to the corresponding <see cref="PictureInfo.PictureHash"/>,
    /// so we don't have to re-decode the same image multiple times
    /// </summary>
    private readonly Dictionary<uint, Bitmap> _cachedImages = new Dictionary<uint, Bitmap>();
}