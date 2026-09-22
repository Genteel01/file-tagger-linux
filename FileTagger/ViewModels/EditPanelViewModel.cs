using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ATL;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileTagger.Assets.Statics;
using FileTagger.Dialogs;
using FileTagger.Extensions;
using FileTagger.Models;
using FileTagger.Services;

namespace FileTagger.ViewModels;

public partial class EditPanelViewModel: ViewModelBase, IRecipient<MainWindowViewModel.SelectedItemsMessage>
{
    /// <summary>
    /// All the tracks that are currently selected
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedTracks))]
    private partial List<TrackViewModel> SelectedTracks { get; set; } = [];

    /// <summary>
    /// Whether SelectedTracks is not empty
    /// </summary>
    public bool HasSelectedTracks => SelectedTracks.Count > 0;

    /// <summary>
    /// The value in the edit box for each field
    /// </summary>
    [ObservableProperty]
    public partial Dictionary<string, string> FieldTexts { get; private set; } = new Dictionary<string, string>();

    /// <summary>
    /// The options in the edit box dropdown for each field
    /// </summary>
    [ObservableProperty]
    public partial Dictionary<string, List<object?>> FieldOptions { get; private set; } = new Dictionary<string, List<object?>>();

    /// <summary>
    /// <see cref="IFileService"/> received through Dependency Injection used for opening the file dialog
    /// </summary>
    private readonly IFileService _fileService;

    /// <summary>
    /// <see cref="IPreferenceService"/> received through Dependency Injection used for getting the initial save location for extracted image
    /// </summary>
    private readonly IPreferenceService _preferenceService;

    /// <summary>
    /// <see cref="IImageService"/> received through Dependency Injection used for handling images
    /// </summary>
    private readonly IImageService _imageService;

    /// <summary>
    /// Function to get the target to use to display a dialog
    /// </summary>
    private readonly Func<TopLevel?>? _getDialogTarget = null;

    /// <summary>
    /// Array of properties of TrackViewModel that we want to be editable
    /// </summary>
    private readonly PropertyInfo[] _trackProperties;

    public EditPanelViewModel(IFileService fileService, IImageService imageService, IPreferenceService preferenceService, Func<TopLevel?> getDialogTarget)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _preferenceService =  preferenceService ?? throw new ArgumentNullException(nameof(preferenceService));
        _imageService =  imageService ?? throw new ArgumentNullException(nameof(imageService));
        IsActive = true;

        //Select properties that are writable and are either string or int?
        _trackProperties = [.. typeof(TrackViewModel).GetProperties().Where(property => property.CanWrite &&
            (property.PropertyType == typeof(string) ||  property.PropertyType == typeof(int?)) )];
        FieldTexts = SetUpFieldTexts();
        FieldOptions = SetUpFieldOptions();
        _getDialogTarget = getDialogTarget;
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
        _preferenceService = new PreferenceService(_fileService);
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
                if (!newFieldOptions[propertyInfo.Name].Contains(propertyInfo.GetValue(track)))
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
                if (FieldTexts[propertyInfo.Name].Trim() == Consts.UnchangedField) continue;

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
        if (images.Count == 0) return;

        if (DisplayedPicture != null)
        {
            ReplaceDisplayedPicture(SelectedTracks, images);
        }
        else
        {
            ReplaceAllPictures(SelectedTracks, images);
        }
        ChooseDisplayedImage();
    }

    /// <summary>
    /// Replaces the currently displayed picture on the given tracks, then adds the new pictures
    /// </summary>
    private void ReplaceDisplayedPicture(List<TrackViewModel> tracks, List<PictureInfo> newImages)
    {
        foreach (TrackViewModel track in tracks)
        {
            int index = RemoveCurrentlyDisplayedPicture(track);
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
    /// Replaces the pictures on the given tracks with the new pictures
    /// </summary>
    private void ReplaceAllPictures(List<TrackViewModel> tracks, List<PictureInfo> newImages)
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

        if (images.Count == 0) return;
        AddPicturesToTracks(SelectedTracks, images);

        ChooseDisplayedImage();
        DisplayedPictureIndex = SelectedTrackPictures.Count - 1;
    }

    /// <summary>
    /// Adds the given pictures to the given tracks
    /// </summary>
    private void AddPicturesToTracks(List<TrackViewModel> tracks, List<PictureInfo> images)
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
    /// Removes the visible picture from the selected tracks, or removes all pictures of the selected type
    /// from all selected tracks if there are different pictures for the selected tracks
    /// </summary>
    [RelayCommand]
    private void RemoveCoverImage()
    {
        if (DisplayedPicture != null)
        {
            RemoveCurrentlyDisplayedPictures(SelectedTracks);
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
    /// Removes the currently displayed pictures from the given tracks
    /// </summary>
    private void RemoveCurrentlyDisplayedPictures(List<TrackViewModel> tracks)
    {
        foreach (TrackViewModel track in tracks)
        {
            RemoveCurrentlyDisplayedPicture(track);
        }
    }

    /// <summary>
    /// Removes the currently displayed image from the given track.
    /// Returns the index of the removed image in the given track's EmbeddedPictures
    /// </summary>
    private int RemoveCurrentlyDisplayedPicture(TrackViewModel track)
    {
        if (DisplayedPicture == null) return -1;
        PictureInfo displayedImage = DisplayedPicture;
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
            if (!HasSelectedTracks || DisplayedPicture == null) return false;
            if (CopiedPic == null) return false;
            if (TracksToCutFrom.Count < SelectedTracks.Count) return false;
            if (!CopiedPic.TrueEqual(DisplayedPicture)) return false;
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

    [RelayCommand(CanExecute = nameof(HasDisplayedPicture))]
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

    [RelayCommand(CanExecute = nameof(HasDisplayedPicture))]
    private void CopyCoverImage()
    {
        CopiedPic = null;
        CopiedPic = DisplayedPicture;
        TracksToCutFrom = [];
    }

    [RelayCommand(CanExecute = nameof(HasImageClipboardData))]
    private void PasteCoverImageAsReplacement()
    {
        if (CopiedPic == null) return;
        FinishCutting();
        CopiedPic.PicType = SelectedPictureType;
        List<PictureInfo> images = [CopiedPic];
        if (DisplayedPicture != null)
        {
            ReplaceDisplayedPicture(SelectedTracks, images);
        }
        else
        {
            ReplaceAllPictures(SelectedTracks, images);
        }
        ChooseDisplayedImage();
    }

    [RelayCommand(CanExecute = nameof(HasImageClipboardData))]
    private void PasteCoverImageAsNew()
    {
        if (CopiedPic == null) return;
        FinishCutting();
        CopiedPic.PicType = SelectedPictureType;
        List<PictureInfo> images = [CopiedPic];
        AddPicturesToTracks(SelectedTracks, images);

        ChooseDisplayedImage();
        DisplayedPictureIndex = SelectedTrackPictures.Count - 1;
    }

    /// <summary>
    /// Chooses which image to display in the edit panel
    /// </summary>
    private void ChooseDisplayedImage()
    {
        int oldDisplayedIndex = DisplayedPictureIndex;
        PictureInfo? oldDisplayedPicture = DisplayedPicture;
        DisplayedPictureIndex = 0;
        SelectedTrackPictures = [];
        SelectedTrackHasPictures = false;
        if (!HasSelectedTracks) return;

        SelectedTrackPictures = GetSelectedTrackPictures();
        if (oldDisplayedPicture != null && SelectedTrackPictures.Count > oldDisplayedIndex)
        {
            if (SelectedTrackPictures[oldDisplayedIndex].TrueEqual(oldDisplayedPicture)) DisplayedPictureIndex = oldDisplayedIndex;
        }
    }

    /// <summary>
    /// Gets the images from SelectedTracks, if all of them have the same images
    /// </summary>
    /// <returns></returns>
    private List<PictureInfo> GetSelectedTrackPictures()
    {
        List<PictureInfo> referencePics = SelectedTracks[0].EmbeddedPictures.Where(MatchesSelectedPicType).ToList();
        if (referencePics.Count != 0) SelectedTrackHasPictures = true;
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
                SelectedTrackHasPictures = true;
            }
            if (relevantPics.Count != referencePics.Count) return [];
            picsPerTrack.Add(relevantPics);
        }

        for (int i = 0; i < referencePics.Count; i++)
        {
            bool allTracksMatch = picsPerTrack.All(pics => pics[i].PicturesEqual(referencePics[i]));
            if (!allTracksMatch) return [];
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
    /// Opens the dialog to edit picture description, and saves the result if it wasn't cancelled
    /// </summary>
    [RelayCommand(CanExecute = nameof(SelectedTrackHasPictures))]
    private async Task OpenPictureDescriptionDialog()
    {
        if (_getDialogTarget?.Invoke() is not Window target) return;

        PicDescriptionDialog dialog = new PicDescriptionDialog();
        string initialText = "";
        if (DisplayedPicture != null) initialText = DisplayedPicture.Description;
        PictureDescriptionViewModel vm = new PictureDescriptionViewModel(dialog, initialText);
        dialog.DataContext = vm;

        string? result = await dialog.ShowDialog<string?>(target);
        if (result == null) return;

        foreach (TrackViewModel track in SelectedTracks)
        {
            if (DisplayedPicture != null)
            {
                int index = track.EmbeddedPictures.FindIndex(pic => pic.TrueEqual(DisplayedPicture));
                track.ChangePictureDescription(result, index);
            }
            else
            {
                for (int i = 0; i < track.EmbeddedPictures.Count; i++)
                {
                    track.ChangePictureDescription(result, i);
                }
            }
        }
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
        DisplayedPictureIndex  = (DisplayedPictureIndex + 1) % SelectedTrackPictures.Count;
    }

    /// <summary>
    /// Show the previous image for the current selected type, looping around
    /// </summary>
    [RelayCommand]
    private void PreviousImage()
    {
        int newIndex = DisplayedPictureIndex - 1;
        if (newIndex < 0) newIndex = SelectedTrackPictures.Count - 1;
        DisplayedPictureIndex = newIndex;
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
    /// Whether any SelectedTrack has pictures of the selected type
    /// </summary>
    [ObservableProperty]
    public partial bool SelectedTrackHasPictures { get; private set; } = false;

    /// <summary>
    /// All EmbeddedPictures for the <see cref="SelectedTracks"/> for the <see cref="SelectedPictureType"/>
    /// if they are the same across all tracks. Will be empty if selected tracks have different pictures
    /// </summary>
    [NotifyPropertyChangedFor(nameof(ShowImageNavigationButtons))]
    [NotifyPropertyChangedFor(nameof(DisplayedPicture))]
    [NotifyPropertyChangedFor(nameof(DisplayedPictureBitmap))]
    [NotifyPropertyChangedFor(nameof(HasDisplayedPicture))]
    [NotifyPropertyChangedFor(nameof(DisplayedImageIsBeingCut))]
    [NotifyPropertyChangedFor(nameof(PicCountString))]
    [ObservableProperty]
    private partial List<PictureInfo> SelectedTrackPictures { get; set; } = [];

    /// <summary>
    /// The index representing which entry in <see cref="SelectedTrackPictures"/> to display
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayedPicture))]
    [NotifyPropertyChangedFor(nameof(DisplayedPictureBitmap))]
    [NotifyPropertyChangedFor(nameof(DisplayedImageIsBeingCut))]
    [NotifyPropertyChangedFor(nameof(PicCountString))]
    private partial int DisplayedPictureIndex { get; set; } = 0;

    /// <summary>
    /// String to indicate how many pictures there are, and which one is being viewed
    /// </summary>
    public string PicCountString => $"{DisplayedPictureIndex + 1}/{SelectedTrackPictures.Count}";

    /// <summary>
    /// Whether to show the navigation buttons for moving between images. Updates when <see cref="SelectedTrackPictures"/> changes
    /// </summary>
    public bool ShowImageNavigationButtons => SelectedTrackPictures.Count > 1;

    /// <summary>
    /// The PictureInfo of the image that is currently being displayed
    /// </summary>
    public PictureInfo? DisplayedPicture => DisplayedPictureIndex < SelectedTrackPictures.Count ? SelectedTrackPictures[DisplayedPictureIndex] : null;

    /// <summary>
    /// Bitmap of <see cref="DisplayedPicture"/>. Updates when SelectedTrackPictures or DisplayedPictureIndex change
    /// </summary>
    public Bitmap? DisplayedPictureBitmap => DisplayedPicture != null ? GetAndCacheBitmap(DisplayedPicture) : null;

    /// <summary>
    /// Whether we are showing the pictures of the selected tracks, or the default image
    /// </summary>
    public bool HasDisplayedPicture => DisplayedPicture != null;

    /// <summary>
    /// List of <see cref="PictureInfo.PIC_TYPE"/> enum values for populating a selection dropdown
    /// </summary>
    public PictureInfo.PIC_TYPE[] PictureTypes { get; } = Enum.GetValues<PictureInfo.PIC_TYPE>();

    /// <summary>
    /// The selected <see cref="PictureInfo.PIC_TYPE"/> from the dropdown
    /// </summary>
    [ObservableProperty]
    public partial PictureInfo.PIC_TYPE SelectedPictureType { get; set; } = PictureInfo.PIC_TYPE.Front;

    /// <summary>
    /// Dictionary of Bitmaps mapped to the corresponding <see cref="PictureInfo.PictureHash"/>,
    /// so we don't have to re-decode the same image multiple times
    /// </summary>
    private readonly Dictionary<uint, Bitmap> _cachedImages = new Dictionary<uint, Bitmap>();

    [RelayCommand]
    private void ChangePictureType(PictureInfo.PIC_TYPE newType)
    {
        foreach (TrackViewModel track in SelectedTracks)
        {
            if (DisplayedPicture != null)
            {
                int index = track.EmbeddedPictures.FindIndex(pic => pic.TrueEqual(DisplayedPicture));
                track.ChangePictureType(newType, index);
            }
            else
            {
                for (int i = 0; i < track.EmbeddedPictures.Count; i++)
                {
                    track.ChangePictureType(newType, i);
                }
            }
        }
        SelectedPictureType = newType;
        ChooseDisplayedImage();
    }

    [RelayCommand(CanExecute = nameof(HasDisplayedPicture))]
    private async Task ExtractCoverImage()
    {
        Bitmap? bitmap = DisplayedPictureBitmap;
        if (bitmap == null || DisplayedPicture == null) return;

        string? bookmarkId = _preferenceService.SystemPreferenceData.LastDirectory;
        string? newBookmarkId = await _fileService.SaveImageFile(bitmap, DisplayedPicture.NativeFormat, DisplayedPicture.PictureHash.ToString(), bookmarkId);

        if (newBookmarkId == null) return;
        PropertyInfo lastDirectoryProperty = typeof(SystemPreferences).GetProperty(nameof(SystemPreferences.LastDirectory))!;
        _preferenceService.StorePreferenceItem(lastDirectoryProperty, newBookmarkId);
    }
}