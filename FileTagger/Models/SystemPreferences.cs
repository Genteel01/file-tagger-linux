namespace FileTagger.Models;

/// <summary>
/// Class that stores settings that are used by the system
/// </summary>
public class SystemPreferences
{
    /// <summary>
    /// Width and height of the main window
    /// </summary>
    public (double Width, double Height) WindowSize { get; set; } = (0, 0);

    /// <summary>
    /// Whether the main window is maximised
    /// </summary>
    public bool IsMaximised { get; set; } = false;
}