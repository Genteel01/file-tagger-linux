using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Styling;
using FileTagger.Assets.Statics;
using FileTagger.ViewModels;

namespace FileTagger.Models;

/// <summary>
/// Class that stores settings that the user has direct control over and might want to reset
/// </summary>
public class UserPreferences
{
    public UserPreferences()
    {
        SortOrder = ($"{nameof(TrackViewModel.Directory)}_{nameof(TrackViewModel.FileName)}", false);
        _storedTheme = ThemeVariant.Default.ToString();
        EditPanelWidth = double.PositiveInfinity;
        ListColumnWidths = new Dictionary<string, double>();
    }
    /// <summary>
    /// String defining which fields the track list is sorted by,
    /// and a bool defining whether they are sorted in descending order
    /// </summary>
    public (string, bool) SortOrder { get; set; }

    /// <summary>
    /// A string representation of the theme that the user has requested.
    /// Storing it as a string for the JSON parser when we save/load
    /// </summary>
    private string _storedTheme;

    /// <summary>
    /// The theme that the user has requested, parsed from the stored string at <see cref="_storedTheme"/>
    /// </summary>
    public ThemeVariant RequestedTheme {
        get => MyThemes.StringToTheme(_storedTheme);
        set => _storedTheme = value.ToString();
    }

    /// <summary>
    /// Widths of each column in the track list
    /// </summary>
    public Dictionary<string, double> ListColumnWidths { get; set
        { field = value; AddMissingColumnWidths(); } }

    /// <summary>
    /// Width of the Edit Panel. Using <see cref="double.PositiveInfinity"/> to represent <see cref="GridLength.Star"/>
    /// </summary>
    public double EditPanelWidth { get; set; }

    /// <summary>
    /// Makes sure <see cref="ListColumnWidths"/> has an entry for each property on <see cref="TrackViewModel"/>
    /// </summary>
    private void AddMissingColumnWidths()
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