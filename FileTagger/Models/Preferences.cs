using FileTagger.ViewModels;

namespace FileTagger.Models;

public class Preferences
{
    /// <summary>
    /// String defining which fields the track list is sorted by,
    /// and a bool defining whether they are sorted in descending order
    /// </summary>
    public (string, bool) SortOrder { get; set; } = (nameof(TrackViewModel.Path), false);

    /// <summary>
    /// Width and height of the main window
    /// </summary>
    public (double, double) WindowSize { get; set; } = (0, 0);

    /// <summary>
    /// Whether the main window is maximised
    /// </summary>
    public bool IsMaximised { get; set; } = false;
}