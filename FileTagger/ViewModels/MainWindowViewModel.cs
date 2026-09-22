using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FileTagger.Services;
using ATL;
using ATL.AudioData;
using ATL.Logging;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileTagger.Assets.Statics;
using FileTagger.Models;

namespace FileTagger.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// All the tracks that have been loaded in
    /// </summary>
    [ObservableProperty]
    public partial List<TrackViewModel> Tracks { get; set; } = [];

    /// <summary>
    /// All the tracks that are currently selected
    /// </summary>
    public ObservableCollection<TrackViewModel> SelectedTracks { get; } = [];

    /// <summary>
    /// Message to send SelectedTracks to <see cref="EditPanelViewModel"/>
    /// </summary>
    /// <param name="Tracks"></param>
    public record SelectedItemsMessage(List<TrackViewModel> Tracks);

    /// <summary>
    /// Reference to EditPanel so we can bind it in our view
    /// </summary>
    public EditPanelViewModel MyEditPanel { get; }

    /// <summary>
    /// <see cref="IFileService"/> received through Dependency Injection used for opening file dialog
    /// </summary>
    private readonly IFileService _fileService;

    /// <summary>
    /// <see cref="IPreferenceService"/> received through Dependency Injection
    /// used for storing and retrieving <see cref="UserPreferences"/> data
    /// </summary>
    private readonly IPreferenceService _preferenceService;

    /// <summary>
    /// List of supported file extensions to fetch from folders
    /// </summary>
    private readonly List<string> _supportedFileExtensions;

    /// <summary>
    /// List of file extensions to do extra checking on when creating tracks
    /// </summary>
    private readonly List<string> _concurrentFileExtensions = [];

    /// <summary>
    /// The current sorting options for Tracks
    /// </summary>
    [ObservableProperty]
    public partial string CurrentSort { get; private set; }

    /// <summary>
    /// Keeps track of whether sorting is ascending or descending
    /// </summary>
    [ObservableProperty]
    public partial bool SortDescending { get; private set; }

    /// <summary>
    /// Widths for each column in the track list
    /// </summary>
    public AvaloniaDictionary<string, double> ListColumnWidths { get; private set; } = new AvaloniaDictionary<string, double>();

    /// <summary>
    /// Width for the Edit Panel
    /// </summary>
    [ObservableProperty]
    public partial GridLength EditPanelWidth { get; set; }

    /// <summary>
    /// List of <see cref="ThemeVariant"/> values to select from
    /// </summary>
    public ThemeVariant[] Themes { get; } = [ThemeVariant.Default, ThemeVariant.Light, MyThemes.LightGreen, ThemeVariant.Dark, MyThemes.DarkGreen];

    /// <summary>
    /// Selected <see cref="ThemeVariant"/>
    /// </summary>
    [ObservableProperty]
    public partial ThemeVariant SelectedTheme { get; private set; } = ThemeVariant.Default;

    [RelayCommand]
    private void ChangeSelectedTheme(ThemeVariant value)
    {
        if (Application.Current is { } app)
        {
            app.RequestedThemeVariant = value;
            SelectedTheme = value;
            PropertyInfo themeProperty = typeof(UserPreferences).GetProperty(nameof(UserPreferences.RequestedTheme))!;
            _preferenceService.StorePreferenceItem(themeProperty, value);
        }
    }

    /// <summary>
    /// Switches between light and dark theme
    /// </summary>
    [RelayCommand]
    private void SwitchTheme()
    {
        ThemeVariant newTheme = MyThemes.GetOppositeTheme(SelectedTheme.ToString());
        ChangeSelectedTheme(newTheme);
    }

    [RelayCommand]
    private void ClearPreferences()
    {
        _preferenceService.ResetUserPreferences();
    }

    public MainWindowViewModel(IFileService fileService, IPreferenceService preferenceService, EditPanelViewModel editPanelViewModel)
    {
        MyEditPanel = editPanelViewModel ?? throw new ArgumentNullException(nameof(editPanelViewModel));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _preferenceService = preferenceService ?? throw new ArgumentNullException(nameof(preferenceService));
        _supportedFileExtensions = [];

        foreach (AudioFormat f in AudioDataIOFactory.GetInstance().getFormats())
        {
            if (f.Readable)
            {
                int id = f.ContainerId != f.DataFormat.ID ? f.ContainerId : f.DataFormat.ID;
                foreach (string extension in f)
                {
                    if (id == AudioDataIOFactory.CID_WMA)
                    {
                        _concurrentFileExtensions.Add(extension.ToLower());
                    }
                    _supportedFileExtensions.Add(extension.ToLower());
                }
            }
        }
        SetUpUserPreferences();
        //Set up event handler to update preferences whenever column widths change
        ListColumnWidths.CollectionChanged += (_, args) =>
        {
            if (args.NewItems == null) return;
            PropertyInfo columnWidthsProperty = typeof(UserPreferences).GetProperty(nameof(UserPreferences.ListColumnWidths))!;
            foreach (KeyValuePair<string, double> newItem in args.NewItems)
            {
                _preferenceService.StorePreferenceDictionaryValue(columnWidthsProperty, newItem.Key, newItem.Value);
            }
        };
        _preferenceService.UserPreferencesSet += (_, _) => SetUpUserPreferences();
        #if DEBUG
        ConsoleLogger dummy = new ConsoleLogger();
        #endif
    }

    /// <summary>
    /// Sets up the data from User Preferences
    /// </summary>
    private void SetUpUserPreferences()
    {
        UserPreferences newPreferences = _preferenceService.UserPreferenceData;
        ListColumnWidths = new AvaloniaDictionary<string, double>(newPreferences.ListColumnWidths);
        EditPanelWidth = double.IsPositiveInfinity(newPreferences.EditPanelWidth) ? GridLength.Star : new GridLength(newPreferences.EditPanelWidth);
        CurrentSort = newPreferences.SortOrder.Item1;
        SortDescending = newPreferences.SortOrder.Item2;
        ThemeVariant loadedTheme = newPreferences.RequestedTheme;
        if (loadedTheme != SelectedTheme) ChangeSelectedTheme(loadedTheme);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(EditPanelWidth))
        {
            //Store EditPanelWidth when it changes
            PropertyInfo editPanelWidthProperty = typeof(UserPreferences).GetProperty(nameof(UserPreferences.EditPanelWidth))!;
            double newValue = EditPanelWidth is { IsStar: true, Value: 1 } ? double.PositiveInfinity : EditPanelWidth.Value;
            _preferenceService.StorePreferenceItem(editPanelWidthProperty, newValue);
        }
    }

    #if DEBUG
    /// <summary>
    /// Default Constructor for design time
    /// </summary>
    public MainWindowViewModel()
    {
        MyEditPanel = new EditPanelViewModel();
        _fileService = new FileService(() => null);
        _preferenceService = new PreferenceService(_fileService);
        _supportedFileExtensions = [];
        CurrentSort = nameof(TrackViewModel.Path);
        ListColumnWidths = new AvaloniaDictionary<string, double>();
        SelectedTheme = ThemeVariant.Default;
    }
    #endif

    /// <summary>
    /// Send the SelectedTracks message when the selection changes
    /// </summary>
    public void SelectionChanged()
    {
        WeakReferenceMessenger.Default.Send(new SelectedItemsMessage(SelectedTracks.ToList()));
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
    /// Stores the Bookmark ID of the chosen directory
    /// </summary>
    [RelayCommand]
    private async Task OpenMusicFiles(CancellationToken token)
    {
        ErrorMessages?.Clear();
        try
        {
            string? lastDirectory = _preferenceService.SystemPreferenceData.LastDirectory;
            (IReadOnlyList<IStorageFile> files, bool cancelled, string? bookmarkId) files = await _fileService.OpenFilesRecursivelyAsync(_supportedFileExtensions, lastDirectory);
            if (files.cancelled) return;

            LoadTracks(files.files);

            if (files.bookmarkId == null) return;
            PropertyInfo lastDirectoryProperty = typeof(SystemPreferences).GetProperty(nameof(SystemPreferences.LastDirectory))!;
            _preferenceService.StorePreferenceItem(lastDirectoryProperty, files.bookmarkId);
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
            throw;
        }
    }

    /// <summary>
    /// Loads the tracks from the bookmarked directory, if there is one
    /// </summary>
    public async Task OpenInitialFiles()
    {
        ErrorMessages?.Clear();
        try
        {
            string? lastDirectory = _preferenceService.SystemPreferenceData.LastDirectory;
            if (lastDirectory == null) return;
            IReadOnlyList<IStorageFile> files = await _fileService.OpenBookmarkedFilesAsync(_supportedFileExtensions, lastDirectory);

            LoadTracks(files);
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
            throw;
        }
    }

    /// <summary>
    /// Loads all tracks in the given list of files
    /// </summary>
    private void LoadTracks(IReadOnlyList<IStorageFile> files)
    {
        List<TrackViewModel> newTracks = [];

        List<Task> tasks = [];
        Lock trackLocker = new Lock();
        Lock wmaLocker = new Lock();

        foreach (IStorageFile file in files)
        {
            Task task = Task.Run(() =>
            {
                //WMA files can hit an error if you try to make two tracks at the same time, so run an extra Lock on them
                string extension = "." + file.Name.Split(".").Last().ToLower();
                if (_concurrentFileExtensions.Contains(extension))
                {
                    Track track;
                    lock (wmaLocker) { track = new Track(file.Path.LocalPath); }
                    TrackViewModel trackViewModel = new TrackViewModel(track);
                    lock (trackLocker) { newTracks.Add(trackViewModel); }
                }
                else
                {
                    Track track = new Track(file.Path.LocalPath);
                    TrackViewModel trackViewModel = new TrackViewModel(track);
                    lock (trackLocker) { newTracks.Add(trackViewModel); }
                }

            });
            tasks.Add(task);
        }
        Task.WaitAll(tasks);

        SelectedTracks.Clear();
        SelectionChanged();
        Tracks = newTracks;
        SortTracks(CurrentSort, false);
    }

    /// <summary>
    /// Sorts tracks by the fields in the order given
    /// </summary>
    /// <param name="fields">String of fields separated by "_", e.g. Album_TrackNumber_Path</param>
    /// <param name="swapDirection">Whether to swap the direction between ascending and descending</param>
    public void SortTracks(string fields, bool swapDirection)
    {
        string[] sortOrder = fields.Split("_");
        List<PropertyInfo> properties = [];

        foreach (string so in sortOrder)
        {
            PropertyInfo? property = typeof(TrackViewModel).GetProperty(so);
            if (property != null)
            {
                properties.Add(property);
            }
        }
        if (properties.Count == 0)
        {
            ErrorMessages?.Add("Sorting by input " + fields + ", which has no valid fields");
            return;
        }

        IOrderedEnumerable<TrackViewModel>? sortedTracks = null;
        foreach (PropertyInfo property in properties)
        {
            object? stringComparer = null;
            if (property.PropertyType == typeof(string))
            {
                stringComparer = property.Name == nameof(TrackViewModel.Path) ?StringComparer.OrdinalIgnoreCase : StringComparer.CurrentCultureIgnoreCase;
            }
            if (sortedTracks == null)
            {
                sortedTracks = Tracks.OrderBy(x => property.GetValue(x), stringComparer as IComparer<object?>);
            }
            else
            {
                sortedTracks = sortedTracks.ThenBy(x => property.GetValue(x), stringComparer as IComparer<object?>);
            }
        }

        if (swapDirection)
        {
            if (CurrentSort != fields)
            {
                SortDescending = false;
            }
            else
            {
                SortDescending = !SortDescending;
            }
        }
        CurrentSort = fields;
        PropertyInfo sortOrderProperty = typeof(UserPreferences).GetProperty(nameof(UserPreferences.SortOrder))!;
        _preferenceService.StorePreferenceItem(sortOrderProperty, (CurrentSort, SortDescending));
        Tracks = (SortDescending ? sortedTracks?.Reverse().ToList() : sortedTracks?.ToList()) ?? [];
    }
}