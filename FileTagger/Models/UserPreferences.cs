using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Styling;
using FileTagger.Statics;
using FileTagger.ViewModels;

namespace FileTagger.Models;

/// <summary>
/// Class that stores settings that the user has direct control over and might want to reset
/// </summary>
public class UserPreferences
{
    public UserPreferences()
    {
        SortOrder = (TrackListData.DefaultSort, false);
        _storedTheme = ThemeVariant.Default.ToString();
        RequestedAccentColour = ColourScheme.Colours.System;
        EditPanelWidth = double.PositiveInfinity;
        ListColumnWidths = new Dictionary<string, double>();
        ListColumnVisibilities = new Dictionary<string, bool>();
        RequestedLayoutSize = Themes.LayoutSize.Automatic;
        FormatString = $"{TextToTagViewModel.PlaceholderChar}{nameof(TrackViewModel.Title)}{TextToTagViewModel.PlaceholderChar}";
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
        get => Themes.StringToTheme(_storedTheme);
        set => _storedTheme = value.ToString();
    }

    /// <summary>
    /// The accent colour that the user has requested
    /// </summary>
    public ColourScheme.Colours RequestedAccentColour { get; set; }

    /// <summary>
    /// Widths of each column in the track list
    /// </summary>
    public Dictionary<string, double> ListColumnWidths { get; set
        { field = value; AddMissingColumnWidths(); } }

    /// <summary>
    /// Visibility of each column in the track list
    /// </summary>
    public Dictionary<string, bool> ListColumnVisibilities { get; set
        { field = value; AddMissingColumnVisibilities(); } }

    /// <summary>
    /// Width of the Edit Panel. Using <see cref="double.PositiveInfinity"/> to represent <see cref="GridLength.Star"/>
    /// </summary>
    public double EditPanelWidth { get; set; }

    public Themes.LayoutSize RequestedLayoutSize { get; set; }

    public string FormatString { get; set; }

    /// <summary>
    /// Makes sure <see cref="ListColumnWidths"/> has an entry for each property on <see cref="TrackViewModel"/>
    /// </summary>
    private void AddMissingColumnWidths()
    {
        foreach ((string name, string _, string _) in TrackListData.PossibleListColumns)
        {
            if (!ListColumnWidths.ContainsKey(name))
            {
                double columnWidth = name switch
                {
                    nameof(TrackViewModel.TrackNumber) or nameof(TrackViewModel.DiscNumber)
                        or nameof(TrackViewModel.Year) => 60,
                    nameof(TrackViewModel.Genre) or nameof(TrackViewModel.Duration) or nameof(TrackViewModel.Bitrate)
                        or nameof(TrackViewModel.SampleRate) => 100,
                    _ => 200
                };
                ListColumnWidths[name] = columnWidth;
            }
        }
    }

    private void AddMissingColumnVisibilities()
    {
        foreach ((string name, string _, string _) in TrackListData.PossibleListColumns)
        {
            ListColumnVisibilities.TryAdd(name, true);
                //TODO temp to test only showing some columns. Should default to true on all columns
                if (name == "AlbumArtist" || name == "Album" || name == "AudioFormat")
                    ListColumnVisibilities[name] = true;
        }
    }
}