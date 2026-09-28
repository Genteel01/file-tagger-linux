using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileTagger.Statics;

namespace FileTagger.ViewModels;

public partial class TextToTagViewModel(Window dialog, List<TrackViewModel> tracks) : ViewModelBase
{
    private const string InvalidFormatMessage = "Invalid format: ({0})";
    private const char PlaceholderChar = '%';

    /// <summary>
    /// The string defining the format to parse
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewText))]
    public partial string FormatString { get; set; } = "";

    /// <summary>
    /// The file name of the displayed preview track
    /// </summary>
    public string PreviewFileName => $"\"{Path.GetFileNameWithoutExtension(tracks[PreviewIndex].FileName)}\" Preview";

    /// <summary>
    /// Gets the preview text for the current format string and selected track index.
    /// Also loads error messages for other tracks
    /// </summary>
    public Dictionary<PropertyInfo, string>? PreviewText {
       get {
            HasValidFormat = false;
            Dictionary<PropertyInfo, string>? newPreview = null;
            if (tracks.Count == 0) return newPreview;
            List<(PropertyInfo property, string delimiter)>? placeholders = ParseFormat(FormatString);
            //With no placeholders the format is invalid
            if (placeholders == null) return newPreview;


            //Check all the tracks for errors
            ErrorMessages.Clear();
            for (int i = 0; i < tracks.Count; i++)
            {
                TrackViewModel track = tracks[i];
                string fileNameNoExtension = Path.GetFileNameWithoutExtension(track.FileName);
                Dictionary<PropertyInfo, string>? trackPreview = TextToTags(placeholders, fileNameNoExtension);
                if (trackPreview != null) HasValidFormat = true;
                //Load the correct track preview
                if (i == PreviewIndex) newPreview = trackPreview;
            }

            //If any tracks have errors, notify the user that they will be skipped.
            //If there is only one track and it has errors, we can't proceed, so don't say it will be skipped.
            if (ErrorMessages.Count > 0 && tracks.Count > 1)
            {
                string counter = ErrorMessages.Count == 1 ? "It" : "They";
                ErrorMessages.Add("");
                ErrorMessages.Add($"{counter} will be skipped");
            }
            return newPreview;
        }
    }

    /// <summary>
    /// The format is considered valid if at least one track can parse it without errors
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyChangesCommand))]
    private partial bool HasValidFormat { get; set; }

    /// <summary>
    /// The index of the track to preview
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewText))]
    [NotifyPropertyChangedFor(nameof(PreviewFileName))]
    private partial int PreviewIndex { get; set; } = 0;

    public bool ShowNavigationButtons => tracks.Count > 1;
    [RelayCommand]
    private void PreviewNext() => PreviewIndex = Maths.ChangeCollectionIndex(PreviewIndex, tracks.Count, 1);
    [RelayCommand]
    private void PreviewPrevious() => PreviewIndex = Maths.ChangeCollectionIndex(PreviewIndex, tracks.Count, -1);


    /// <summary>
    /// Extracts the placeholders from the format string and returns a list of their property and delimiter
    /// </summary>
    private List<(PropertyInfo property, string delimiter)>? ParseFormat(string formatString)
    {
        ErrorMessages.Clear();
        // Find all placeholders in the format string
        List<(PropertyInfo property, string delimiter)> placeholders = [];
        int pos = 0;
        while (pos < formatString.Length)
        {
            int start = formatString.IndexOf(PlaceholderChar, pos);
            if (start == -1)
            {
                ErrorMessages.Add(string.Format(InvalidFormatMessage, $"Missing Opening \'{PlaceholderChar}\'"));
                return null;
            }

            int end = formatString.IndexOf(PlaceholderChar, start + 1);
            if (end == -1)
            {
                ErrorMessages.Add(string.Format(InvalidFormatMessage, $"Missing Closing \'{PlaceholderChar}\'"));
                return null;
            }

            int nextStart = formatString.IndexOf(PlaceholderChar, end + 1);
            string delimiter = "";
            if (nextStart != -1)
            {
                delimiter = formatString.Substring(end + 1, nextStart - end - 1);
            }

            string placeholderName = formatString.Substring(start + 1, end - start - 1);
            if (placeholderName.Length == 0)
            {
                ErrorMessages.Add(string.Format(InvalidFormatMessage, "Missing Tag Name"));
                return null;
            }
            PropertyInfo? property = PropertyHelpers.GetProperty<TrackViewModel>(placeholderName);

            if (property == null)
            {
                ErrorMessages.Add(string.Format(InvalidFormatMessage, $"Invalid Tag Name \"{placeholderName}\""));
                return null;
            }

            if (!property.CanWrite)
            {
                ErrorMessages.Add(string.Format(InvalidFormatMessage, $"Tag \"{placeholderName}\" Cannot Be Written"));
                return null;
            }

            placeholders.Add((property, delimiter));
            pos = end + 1;
        }

        if (placeholders.Count == 0)
        {
            ErrorMessages.Add(string.Format(InvalidFormatMessage, "No Placeholders"));
            return null;
        }

        return placeholders;
    }

    /// <summary>
    /// Applies the placeholders to the given text and returns a dictionary of the properties and their values.
    /// Returns null if the format is invalid for the given text.
    /// </summary>
    private Dictionary<PropertyInfo, string>? TextToTags(List<(PropertyInfo property, string delimiter)> placeholders, string text)
    {
        // Build a pattern to extract values
        Dictionary<PropertyInfo, string> values = new Dictionary<PropertyInfo, string>();
        int filePos = 0;

        foreach ((PropertyInfo property, string delimiter) in placeholders)
        {
            string value;
            //Empty delimiter is after the last placeholder, so use the rest of the text
            if (string.IsNullOrEmpty(delimiter))
            {
                value = text.Substring(filePos);
                filePos = text.Length;
            }
            else
            {
                int delimiterPos = text.IndexOf(delimiter, filePos, StringComparison.Ordinal);
                if (delimiterPos == -1)
                {
                    // If no delimiter is found, the format is not valid
                    ErrorMessages.Add($"Format doesn't match \"{text}\"");
                    return null;
                }

                value = text.Substring(filePos, delimiterPos - filePos);
                filePos = delimiterPos + delimiter.Length;
            }

            // If it's an int field and the value can't be parsed, the format is not valid
            if (property.PropertyType == typeof(int?))
            {
                bool parsed = int.TryParse(value, out int parsedInt);
                if (!parsed)
                {
                    ErrorMessages.Add($"Format doesn't match \"{text}\":");
                    ErrorMessages.Add($"    Invalid value for \"{property.Name}\"");
                    return null;
                }
            }

            values[property] = value.Trim();
        }

        return values;
    }

    /// <summary>
    /// Applies the format to all the tracks.
    /// Skips tracks that have errors.
    /// Can be run if at least one track will pass without errors.
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasValidFormat))]
    private void ApplyChanges()
    {
        foreach (TrackViewModel track in tracks)
        {
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(track.FileName);
            List<(PropertyInfo property, string delimiter)>? placeholders = ParseFormat(FormatString);
            if (placeholders == null) continue;
            Dictionary<PropertyInfo, string>? properties = TextToTags(placeholders, fileNameNoExtension);
            if (properties == null) continue;
            foreach (PropertyInfo property in properties.Keys)
            {
                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(track, properties[property]);
                }
                else if (property.PropertyType == typeof(int?))
                {
                    bool parsed = int.TryParse(properties[property], out int parsedInt);
                    if (parsed) property.SetValue(track, parsedInt);
                    else property.SetValue(track, null);
                }
            }
        }

        dialog.Close();
    }

    [RelayCommand]
    private void Cancel() => dialog.Close();
}