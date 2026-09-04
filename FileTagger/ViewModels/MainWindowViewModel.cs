using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FileTagger.Services;
using ATL;
using ATL.AudioData;
using ATL.Logging;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

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
    /// List of supported file extensions to fetch from folders
    /// </summary>
    private readonly List<string> _supportedFileExtensions;

    public MainWindowViewModel(IFileService fileService, EditPanelViewModel editPanelViewModel)
    {
        MyEditPanel = editPanelViewModel ?? throw new ArgumentNullException(nameof(editPanelViewModel));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
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
    }

    #if DEBUG
    /// <summary>
    /// Default Constructor for design time
    /// </summary>
    public MainWindowViewModel()
    {
        MyEditPanel = new EditPanelViewModel();
        _fileService = new FileService(new Window());
        _supportedFileExtensions = [];
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
            newTracks.Sort((x, y) => string.Compare(x.Path, y.Path, StringComparison.OrdinalIgnoreCase));
            SelectedTracks.Clear();
            SelectionChanged();
            Tracks = newTracks;
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }
    }
}