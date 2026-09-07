using FileTagger.ViewModels;

namespace FileTagger.Models;

public class Preferences
{
    /// <summary>
    /// How the track list is sorted
    /// </summary>
    public string SortOrder { get; set; } = nameof(TrackViewModel.Path);

    public bool SortDescending { get; set; } = false;
}