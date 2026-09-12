using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ATL;
using ATL.AudioData;
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
        IEnumerable<string> picTypes = Enum.GetNames<PictureInfo.PIC_TYPE>();
        PictureTypes = [.. picTypes];

        SelectedPictureType = Enum.GetName(PictureInfo.PIC_TYPE.Front)!;

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
        PictureTypes = [];
        _selectedPictureType = "";
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
    /// Opens the file picker for the user to select new images,
    /// then replaces the cover images of the currently selected type, for the currently selected tracks
    /// </summary>
    [RelayCommand]
    private async Task ReplaceCoverImage(CancellationToken token)
    {
        PictureInfo.PIC_TYPE pictureType = Enum.Parse<PictureInfo.PIC_TYPE>(SelectedPictureType);
        List<(Bitmap, PictureInfo)> images = await SelectImageFiles(pictureType);

        if(images.Count == 0) return;
        foreach (TrackViewModel track in SelectedTracks)
        {
            track.EmbeddedPictures.RemoveAll(pic => pic.PicType == pictureType);
            track.EmbeddedPictures.AddRange(images.Select(pic => pic.Item2));
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
        PictureInfo.PIC_TYPE pictureType = Enum.Parse<PictureInfo.PIC_TYPE>(SelectedPictureType);
        List<(Bitmap, PictureInfo)> images = await SelectImageFiles(pictureType);

        if(images.Count == 0) return;
        foreach (TrackViewModel track in SelectedTracks)
        {
            track.EmbeddedPictures.AddRange(images.Select(pic => pic.Item2));
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
    /// Opens the file picker to select image files, and returns a list of (<see cref="Bitmap"/>, <see cref="PictureInfo"/>) for every image
    /// </summary>
    private async Task<List<(Bitmap, PictureInfo)>> SelectImageFiles(PictureInfo.PIC_TYPE pictureType)
    {
        ErrorMessages?.Clear();
        try
        {
            if (_fileService is null) throw new NullReferenceException("Missing File Service instance.");

            IReadOnlyList<IStorageFile> files = await _fileService.OpenImageFiles();

            List<(Bitmap, PictureInfo)> images = [];

            for (int i = 0; i < files.Count; i++)
            {
                IStorageFile file = files[i];
                await using Stream stream = await file.OpenReadAsync();
                Bitmap bitmap = new Bitmap(stream);
                stream.Seek(0, SeekOrigin.Begin);
                PictureInfo picInfo = PictureInfo.fromBinaryData(stream, (int)stream.Length,
                    pictureType, MetaDataIOFactory.TagType.ANY, 0, i + 1);
                await stream.DisposeAsync();
                images.Add((bitmap, picInfo));
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
        PictureInfo.PIC_TYPE pictureType = Enum.Parse<PictureInfo.PIC_TYPE>(SelectedPictureType);
        if (CurrentDisplayedImage == _imageService.GetDefaultImage())
        {
            foreach (TrackViewModel track in SelectedTracks)
            {
                if(track.EmbeddedPictures.Count != 0) track.Changed = true;
                track.EmbeddedPictures.RemoveAll(pic => pic.PicType == pictureType);
            }
        }
        else
        {
            foreach (TrackViewModel track in SelectedTracks)
            {
                int oldCount = track.EmbeddedPictures.Count;
                (Bitmap, PictureInfo) displayedImage = SelectedTrackImages[DisplayedImageIndex];
                PictureInfo matchingImage = track.EmbeddedPictures.First(pic => ArePicturesIdentical(displayedImage.Item2.PictureData, pic.PictureData) && pic.Equals(displayedImage.Item2));
                track.EmbeddedPictures.Remove(matchingImage);
                int newCount = track.EmbeddedPictures.Count;
                if(oldCount != newCount) track.Changed = true;
            }
        }
        ChooseDisplayedImage();
    }

    /// <summary>
    /// Chooses which image to display in the edit panel
    /// </summary>
    private void ChooseDisplayedImage()
    {
        DisplayedImageIndex = 0;
        if (!HasSelectedTracks)
        {
            SelectedTrackImages = [];
            return;
        }
        List<PictureInfo> newSelectedTrackImages = [];

        PictureInfo.PIC_TYPE pictureType = Enum.Parse<PictureInfo.PIC_TYPE>(SelectedPictureType);

        //If there's only one track selected, display all its images
        if (SelectedTracks.Count == 1)
        {
            IEnumerable<PictureInfo> validPics = SelectedTracks.First().EmbeddedPictures.Where(pic => pic.PicType == pictureType);
            newSelectedTrackImages = [.. validPics];
        }
        //If there is more than one track selected, display its images if they are the same across the entire selection
        //Only proceed if all selected tracks actually have an image of the current type
        else if (SelectedTracks.All(track => track.EmbeddedPictures.Any(pic => pic.PicType == pictureType)))
        {
            //Get a list of the pics of the correct type for each selected track
            List<List<PictureInfo>> validPicsPerTrack = [];
            foreach (TrackViewModel track in SelectedTracks)
            {
                validPicsPerTrack.Add([.. track.EmbeddedPictures.Where(pic => pic.PicType == pictureType)]);
            }
            //If each selected track doesn't have the same number of pics, we already know they don't match and can move on
            int firstTrackImageCount = validPicsPerTrack.First().Count;
            bool matchingSizes = validPicsPerTrack.All(trackPics => trackPics.Count== firstTrackImageCount);
            if (matchingSizes)
            {
                //Loop through each picture, and check whether it is the same on each selected track
                bool picsMatch = true;
                for (int i = 0; i < firstTrackImageCount; i++)
                {
                    byte[] firstTrackPicData = validPicsPerTrack.First()[i].PictureData;
                    if (!validPicsPerTrack.All(trackPics => ArePicturesIdentical(firstTrackPicData, trackPics[i].PictureData)))
                    {
                        picsMatch = false;
                        break;
                    }
                }

                if (picsMatch)
                {
                    //If all pics match, display them all
                    newSelectedTrackImages = [.. validPicsPerTrack.First()];
                }
            }
        }

        List<(Bitmap, PictureInfo)> newResolvedImages = [];
        foreach (PictureInfo picInfo in newSelectedTrackImages)
        {
            //TODO Should probably cache the Bitmaps by storing them on the SelectedImage.EmbeddedPictures
            newResolvedImages.Add((new Bitmap(new MemoryStream(picInfo.PictureData)), picInfo));
        }
        SelectedTrackImages = newResolvedImages;
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
    /// Choose the correct image to display when <see cref="SelectedPictureType"/> changes
    /// </summary>
    // ReSharper disable once UnusedParameterInPartialMethod
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
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ShowImageNavigationButtons))] [NotifyPropertyChangedFor(nameof(CurrentDisplayedImage))]
    private List<(Bitmap, PictureInfo)> _selectedTrackImages = [];

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
    public Bitmap CurrentDisplayedImage => SelectedTrackImages.Count > 0 ? SelectedTrackImages[DisplayedImageIndex].Item1 : _imageService.GetDefaultImage();

    /// <summary>
    /// Whether to show the navigation buttons for moving between images. Updates when <see cref="SelectedTrackImages"/> changes
    /// </summary>
    public bool ShowImageNavigationButtons => SelectedTrackImages.Count > 1;

    /// <summary>
    /// List of <see cref="PictureInfo.PIC_TYPE"/> enum values as strings, for populating a selection dropdown
    /// </summary>
    public List<string> PictureTypes { get; }

    /// <summary>
    /// String of the selected <see cref="PictureInfo.PIC_TYPE"/> from the dropdown
    /// </summary>
    [ObservableProperty] private string _selectedPictureType;
}