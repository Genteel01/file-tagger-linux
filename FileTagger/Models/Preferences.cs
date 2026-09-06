using FileTagger.ViewModels;

namespace FileTagger.Models;

public class Preferences
{
    /// <summary>
    /// How the track list is sorted
    /// </summary>
    public (string, bool) SortOrder { get; set; } = (nameof(TrackViewModel.Path), false);
}