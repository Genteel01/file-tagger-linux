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

public partial class EmbeddedPictureViewModel : ViewModelBase, IRecipient<MainWindowViewModel.SelectedItemsMessage>
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
    /// Whether we have a copied image
    /// </summary>
    public bool HasImageClipboardData => CopiedPic != null;

    /// <summary>
    /// Decides whether to show the display image as being cut.
    /// Is true if <see cref="TracksToCutFrom"/> contains all of <see cref="SelectedTracks"/> and the displayed image is <see cref="CopiedPic"/>
    /// </summary>
    public bool DisplayedPictureIsBeingCut
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
    [NotifyPropertyChangedFor(nameof(DisplayedPictureIsBeingCut))]
    [NotifyPropertyChangedFor(nameof(PicCountString))]
    [ObservableProperty]
    private partial List<PictureInfo> SelectedTrackPictures { get; set; } = [];

    /// <summary>
    /// The index representing which entry in <see cref="SelectedTrackPictures"/> to display
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayedPicture))]
    [NotifyPropertyChangedFor(nameof(DisplayedPictureBitmap))]
    [NotifyPropertyChangedFor(nameof(DisplayedPictureIsBeingCut))]
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
    /// The PictureInfo of the image that is currently being displayed. When it is null, Picture operations apply to all
    /// EmbeddedPictures on the selected tracks
    /// </summary>
    public PictureInfo? DisplayedPicture => DisplayedPictureIndex < SelectedTrackPictures.Count ? SelectedTrackPictures[DisplayedPictureIndex] : null;

    /// <summary>
    /// Bitmap of <see cref="DisplayedPicture"/>. Updates when SelectedTrackPictures or DisplayedPictureIndex change
    /// </summary>
    public Bitmap? DisplayedPictureBitmap => DisplayedPicture != null ? _imageService.GetBitmap(DisplayedPicture) : null;

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
    /// List of tracks we are cutting images from
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayedPictureIsBeingCut))]
    private partial List<TrackViewModel> TracksToCutFrom { get; set; } = [];

    /// <summary>
    /// The currently copied image, if there is one
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasImageClipboardData))]
    private partial PictureInfo? CopiedPic { get; set; } = null;

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

    public EmbeddedPictureViewModel(IFileService fileService, IImageService imageService, IPreferenceService preferenceService, Func<TopLevel?> getDialogTarget)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _preferenceService =  preferenceService ?? throw new ArgumentNullException(nameof(preferenceService));
        _imageService =  imageService ?? throw new ArgumentNullException(nameof(imageService));
        _getDialogTarget = getDialogTarget;
        IsActive = true;
    }
    #if DEBUG
    /// <summary>
    /// Default Constructor for design time
    /// </summary>
    public EmbeddedPictureViewModel()
    {
        _fileService = new FileService(() => null);
        _imageService = new ImageService();
        _preferenceService = new PreferenceService(_fileService);
    }
    #endif

    public void Receive(MainWindowViewModel.SelectedItemsMessage message)
    {
        SelectedTracks = message.Tracks;
        ChooseDisplayedImage();
    }

    /// <summary>
    /// Predicate for <see cref="IEnumerable{T}"/> LINQ expressions to check
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

        ReplacePictures(images);
    }

    private void ReplacePictures(List<PictureInfo> newPictures)
    {
        foreach (TrackViewModel track in SelectedTracks)
        {
            int index = -1;
            //Remove either the currently displayed picture or all pictures
            if(DisplayedPicture != null) index = RemoveCurrentlyDisplayedPicture(track);
            else track.EmbeddedPictures.RemoveAll(MatchesSelectedPicType);
            //Add the new pictures either in place or on the end
            if (index == -1) track.EmbeddedPictures.AddRange(newPictures);
            else track.EmbeddedPictures.InsertRange(index, newPictures);
        }
        ChooseDisplayedImage();
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
        AddPicturesToEnd(images);
    }

    /// <summary>
    /// Adds the given pictures to the Selected Tracks
    /// </summary>
    private void AddPicturesToEnd(List<PictureInfo> newPictures)
    {
        foreach (TrackViewModel track in SelectedTracks)
        {
            track.EmbeddedPictures.AddRange(newPictures);
        }
        ChooseDisplayedImage();
        DisplayedPictureIndex = SelectedTrackPictures.Count - 1;
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
        foreach (TrackViewModel track in SelectedTracks)
        {
            if(DisplayedPicture != null) RemoveCurrentlyDisplayedPicture(track);
            else track.EmbeddedPictures.RemoveAll(MatchesSelectedPicType);
        }
        ChooseDisplayedImage();
    }

    /// <summary>
    /// Removes the currently displayed image from the given track.
    /// Returns the index of the removed image in the given track's EmbeddedPictures
    /// </summary>
    private int RemoveCurrentlyDisplayedPicture(TrackViewModel track)
    {
        if (DisplayedPicture == null) return -1;
        int index = track.EmbeddedPictures.Remove(pic => pic.TrueEqual(DisplayedPicture));
        return index;
    }

    [RelayCommand(CanExecute = nameof(HasDisplayedPicture))]
    private void CutCoverImage()
    {
        CopyCoverImage();
        TracksToCutFrom = [.. SelectedTracks];
    }

    /// <summary>
    /// Removes the image that was cut from the tracks it was cut from
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
        PasteCoverImage(true);
    }

    [RelayCommand(CanExecute = nameof(HasImageClipboardData))]
    private void PasteCoverImageAsNew()
    {
        PasteCoverImage();
    }

    private void PasteCoverImage(bool replaceExisting = false)
    {
        if (CopiedPic == null) return;
        FinishCutting();
        PictureInfo newPic = new PictureInfo(CopiedPic);
        newPic.PicType = SelectedPictureType;
        List<PictureInfo> images = [newPic];
        if(replaceExisting) ReplacePictures(images);
        else AddPicturesToEnd(images);
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
        DisplayedPictureIndex = Maths.ChangeCollectionIndex(DisplayedPictureIndex, SelectedTrackPictures.Count, 1);
    }

    /// <summary>
    /// Show the previous image for the current selected type, looping around
    /// </summary>
    [RelayCommand]
    private void PreviousImage()
    {
        DisplayedPictureIndex = Maths.ChangeCollectionIndex(DisplayedPictureIndex, SelectedTrackPictures.Count, -1);
    }

    /// <summary>
    /// Switch to the next picture type, looping around
    /// </summary>
    [RelayCommand]
    private void NextPicType()
    {
        ChangeSelectedPictureType(1);
    }

    /// <summary>
    /// Switch to the previous picture type, looping around
    /// </summary>
    [RelayCommand]
    private void PreviousPicType()
    {
        ChangeSelectedPictureType(-1);
    }

    private void ChangeSelectedPictureType(int change)
    {
        int currentIndex = PictureTypes.IndexOf(SelectedPictureType);
        if (currentIndex == -1) return;
        SelectedPictureType = PictureTypes[DisplayedPictureIndex = Maths.ChangeCollectionIndex(currentIndex, PictureTypes.Length, change)];
    }

    [RelayCommand(CanExecute = nameof(SelectedTrackHasPictures))]
    private void ChangePictureType(PictureInfo.PIC_TYPE newType)
    {
        ModifySelectedTracksPictures((track, index) => track.ChangePictureType(newType, index));
        SelectedPictureType = newType;
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

        ModifySelectedTracksPictures((track, index) => track.ChangePictureDescription(result, index));
        //Force an update so the binding reads the new description
        ChooseDisplayedImage();
    }

    /// <summary>
    /// Performs an operation on the displayed picture, or all pictures when no individual picture is displayed.
    /// </summary>
    private void ModifySelectedTracksPictures(Action<TrackViewModel, int> action)
    {
        foreach (TrackViewModel track in SelectedTracks)
        {
            if (DisplayedPicture != null)
            {
                int index = track.EmbeddedPictures.FindIndex(pic => pic.TrueEqual(DisplayedPicture));
                action(track, index);
            }
            else
            {
                for (int i = 0; i < track.EmbeddedPictures.Count; i++)
                {
                    action(track, i);
                }
            }
        }
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