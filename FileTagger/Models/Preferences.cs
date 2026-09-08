using FileTagger.ViewModels;

namespace FileTagger.Models;

public class Preferences
{
    /// <summary>
    /// How the track list is sorted
    /// </summary>
    public string SortOrder { get; set; } = nameof(TrackViewModel.Path);

    public bool SortDescending { get; set; } = false;

    /// <summary>
    /// Width and height of the main window
    /// </summary>
    public (double, double) WindowSize { get; set; } = (0, 0);

    /// <summary>
    /// Whether the main window is maximised
    /// </summary>
    public bool IsMaximised { get; set; } = false;
}