using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Styling;
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
    public (double Width, double Height) WindowSize { get; set; } = (0, 0);

    /// <summary>
    /// Whether the main window is maximised
    /// </summary>
    public bool IsMaximised { get; set; } = false;

    /// <summary>
    /// Widths of each column in the track list
    /// </summary>
    public Dictionary<string, double> ListColumnWidths { get; set; } = new Dictionary<string, double>();

    /// <summary>
    /// Width of the Edit Panel. Using <see cref="double.PositiveInfinity"/> to represent <see cref="GridLength.Star"/>
    /// </summary>
    public double EditPanelWidth { get; set; } = double.PositiveInfinity;

    /// <summary>
    /// A string representation of the theme that the user has requested
    /// </summary>
    public string RequestedTheme { get; set; } = ThemeVariant.Default.ToString();

    /// <summary>
    /// Gets the <see cref="ThemeVariant"/> corresponding to <see cref="RequestedTheme"/>
    /// </summary>
    /// <returns></returns>
    public ThemeVariant GetStoredTheme()
    {
        switch (RequestedTheme)
        {
            case "Light":
                return ThemeVariant.Light;
            case "Dark":
                return ThemeVariant.Dark;
            default:
                return ThemeVariant.Default;
        }
    }

    /// <summary>
    /// Makes sure <see cref="ListColumnWidths"/> has an entry for each property on <see cref="TrackViewModel"/>
    /// </summary>
    public void AddMissingColumnWidths()
    {
        List<PropertyInfo> trackProperties = [.. typeof(TrackViewModel).GetProperties().Where(property => property.PropertyType == typeof(string) ||  property.PropertyType == typeof(int?) )];
        foreach (PropertyInfo trackProperty in trackProperties)
        {
            if (!ListColumnWidths.ContainsKey(trackProperty.Name))
            {
                double columnWidth = trackProperty.PropertyType == typeof(string) ? 200 : 60;
                if (trackProperty.Name == nameof(TrackViewModel.Genre))
                {
                    columnWidth = 100;
                }
                ListColumnWidths[trackProperty.Name] = columnWidth;
            }
        }
    }
}