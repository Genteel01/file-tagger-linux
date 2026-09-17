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
using FileTagger.Models;

namespace FileTagger.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// All the tracks that have been loaded in
    /// </summary>
    [ObservableProperty]
    private List<TrackViewModel> _tracks = [];

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
    /// used for storing and retrieving <see cref="Preferences"/> data
    /// </summary>
    private readonly IPreferenceService _preferenceService;

    /// <summary>
    /// List of supported file extensions to fetch from folders
    /// </summary>
    private readonly List<string> _supportedFileExtensions;

    /// <summary>
    /// The current sorting options for Tracks
    /// </summary>
    [ObservableProperty]
    private string _currentSort;

    /// <summary>
    /// Keeps track of whether sorting is ascending or descending
    /// </summary>
    [ObservableProperty]
    private bool _sortDescending;

    /// <summary>
    /// Widths for each column in the track list
    /// </summary>
    public AvaloniaDictionary<string, double> ListColumnWidths { get; set; }

    /// <summary>
    /// Width for the Edit Panel
    /// </summary>
    [ObservableProperty]
    private GridLength _editPanelWidth;

    /// <summary>
    /// List of <see cref="ThemeVariant"/> values to select from
    /// </summary>
    public ThemeVariant[] Themes { get; }

    /// <summary>
    /// Selected <see cref="ThemeVariant"/>
    /// </summary>
    [ObservableProperty] private ThemeVariant _selectedTheme;

    [RelayCommand]
    private void ChangeSelectedTheme(ThemeVariant value)
    {
        if (Application.Current is { } app)
        {
            SelectedTheme = value;
            app.RequestedThemeVariant = value;
        }
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
                foreach (string extension in f)
                {
                    _supportedFileExtensions.Add(extension.ToLower());
                }
            }
        }
        //Load initial sort settings
        Preferences preferences = _preferenceService.GetPreferenceData();
        CurrentSort = preferences.SortOrder.Item1;
        SortDescending = preferences.SortOrder.Item2;
        //Load initial column widths
        ListColumnWidths = new AvaloniaDictionary<string, double>(preferences.ListColumnWidths);
        //Set up event handler to update preferences whenever column widths change
        ListColumnWidths.CollectionChanged += (_, args) =>
        {
            if (args.NewItems == null) return;
            PropertyInfo columnWidthsProperty = typeof(Preferences).GetProperty(nameof(Preferences.ListColumnWidths))!;
            foreach (KeyValuePair<string, double> newItem in args.NewItems)
            {
                _preferenceService.StorePreferenceDictionaryValue(columnWidthsProperty, newItem.Key, newItem.Value);
            }
        };
        //Load initial EditPanel width
        EditPanelWidth = double.IsPositiveInfinity(preferences.EditPanelWidth) ? GridLength.Star : new GridLength(preferences.EditPanelWidth);
        Themes = [ThemeVariant.Default, ThemeVariant.Light, ThemeVariant.Dark];
        SelectedTheme = Application.Current?.RequestedThemeVariant ?? ThemeVariant.Default;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(EditPanelWidth))
        {
            //Store EditPanelWidth when it changes
            PropertyInfo editPanelWidthProperty = typeof(Preferences).GetProperty(nameof(Preferences.EditPanelWidth))!;
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
        Themes = [ThemeVariant.Default, ThemeVariant.Light, ThemeVariant.Dark];
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
    /// </summary>
    [RelayCommand]
    private async Task OpenMusicFiles(CancellationToken token)
    {
        #if DEBUG
        ConsoleLogger dummy = new ConsoleLogger();
        #endif
        ErrorMessages?.Clear();
        try
        {
            (IReadOnlyList<IStorageFile>, bool) files = await _fileService.OpenFilesRecursivelyAsync(_supportedFileExtensions);
            if (files.Item2) return;

            List<TrackViewModel> newTracks = [];
            //Never have more chunks than the number of threads available on the system, but also there's overhead in
            //creating threads, so we don't want our chunks to be too small, because that means we run more threads
            //Using 32 as an arbitrary minimum chunk size
            int chunkSize = (int)MathF.Ceiling((float)files.Item1.Count / Environment.ProcessorCount);
            IEnumerable<IStorageFile[]> chunkedFiles = files.Item1.Chunk(Math.Max(chunkSize, 32));

            List<Thread> threads = [];
            object trackLocker = new object();

            foreach (IStorageFile[] fileSubset in chunkedFiles)
            {
                Thread t = new Thread(() =>
                {
                    foreach (IStorageFile file in fileSubset)
                    {
                        Track track = new Track(file.Path.LocalPath);
                        TrackViewModel trackViewModel = new TrackViewModel(track);
                        lock (trackLocker)
                        {
                            newTracks.Add(trackViewModel);
                        }
                    }
                });
                threads.Add(t);
                t.Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            SelectedTracks.Clear();
            SelectionChanged();
            Tracks = newTracks;
            SortTracks(CurrentSort, false);
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }
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
        PropertyInfo sortOrderProperty = typeof(Preferences).GetProperty(nameof(Preferences.SortOrder))!;
        _preferenceService.StorePreferenceItem(sortOrderProperty, (CurrentSort, SortDescending));
        Tracks = (SortDescending ? sortedTracks?.Reverse().ToList() : sortedTracks?.ToList()) ?? [];
    }
}