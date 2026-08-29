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

    public MainWindowViewModel(IFileService fileService, EditPanelViewModel editPanelViewModel)
    {
        MyEditPanel = editPanelViewModel ?? throw new ArgumentNullException(nameof(editPanelViewModel));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    #if DEBUG
    /// <summary>
    /// Default Constructor for design time
    /// </summary>
    public MainWindowViewModel()
    {
        MyEditPanel = new EditPanelViewModel();
        _fileService = new FileService(new Window());
    }
    #endif

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
        ConsoleLogger log = new ConsoleLogger();
        ErrorMessages?.Clear();
        try
        {
            (IReadOnlyList<IStorageFile>, bool) files = await _fileService.OpenFilesRecursivelyAsync();
            if (files.Item2) return;

            List<TrackViewModel> newTracks = [];
            SelectedTracks.Clear();
            SelectionChanged();
            foreach (IStorageFile file in files.Item1)
            {
                string fileName = file.Name.ToLower();
                //TODO do this checking against ATL's supported types
                if (fileName.EndsWith(".mp3") ||  fileName.EndsWith(".wav") || fileName.EndsWith(".flac"))
                {
                    Track track = new Track(file.Path.LocalPath);
                    newTracks.Add(new TrackViewModel(track));
                }
            }
            Tracks = newTracks;
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }
    }
}