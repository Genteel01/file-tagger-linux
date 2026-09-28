using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileTagger.Services;
using FileTagger.Statics;

namespace FileTagger.ViewModels;

public partial class TextToTagViewModel(Window dialog, List<TrackViewModel> tracks, IFileService fileService) : ViewModelBase
{
    private const string InvalidFormatMessage = "Invalid format: ({0})";
    private const char PlaceholderChar = '%';

    /// <summary>
    /// Whether we are loading tags from a text file
    /// </summary>
    public bool LoadFromFile { get; set; } = false;

    /// <summary>
    /// The lines of the loaded text file
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewText))]
    [NotifyPropertyChangedFor(nameof(HasFile))]
    private partial List<string>? FileLines { get; set; } = null;

    /// <summary>
    /// Whether we have loaded a file
    /// </summary>
    public bool HasFile => FileLines != null;

    /// <summary>
    /// The name of the loaded file
    /// </summary>
    [ObservableProperty]
    public partial string FileName { get; set; } = "No File Selected";

    /// <summary>
    /// List of warning messages regarding the loaded file
    /// </summary>
    [ObservableProperty]
    public partial List<string> FileWarnings { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ViewHeight))]
    public partial List<Message> ParseMessages { get; set; } = [];

    /// <summary>
    /// Array of properties of TrackViewModel that we want to be valid in the format
    /// </summary>
    private PropertyInfo[] TrackProperties { get; } = TrackViewModel.GetEditableProperties();

    public string[] TrackPropertyNames => TrackProperties.Select(property => property.Name).ToArray();

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
            ErrorMessages.Clear();
            Dictionary<PropertyInfo, string>? newPreview = null;
            if (tracks.Count == 0) return newPreview;

            if (LoadFromFile)
            {
                if (FileLines == null)
                {
                    ErrorMessages.Add("No file selected");
                    return newPreview;
                }
                if (FileLines.Count == 0)
                {
                    ErrorMessages.Add("File is empty");
                    return newPreview;
                }
            }

            List<(PropertyInfo property, string delimiter)>? placeholders = ParseFormat(FormatString);
            //With no placeholders the format is invalid
            if (placeholders == null) return newPreview;

            //Check all the tracks for errors
            int numberOfEntries = LoadFromFile ? Math.Min(tracks.Count, FileLines!.Count) : tracks.Count;
            for (int i = 0; i < numberOfEntries; i++)
            {
                TrackViewModel track = tracks[i];
                string textSource = LoadFromFile ? FileLines![i] : Path.GetFileNameWithoutExtension(track.FileName);
                Dictionary<PropertyInfo, string>? trackPreview = TextToTags(placeholders, textSource);
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

            if (property == null || !TrackProperties.Contains(property))
            {
                ErrorMessages.Add(string.Format(InvalidFormatMessage, $"Invalid Tag Name \"{placeholderName}\""));
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
    /// Adds the given string to the format string, surrounded by the Placeholder Character
    /// </summary>
    public void AddProperty(string propertyName)
    {
        FormatString += $"{PlaceholderChar}{propertyName}{PlaceholderChar}";
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

    [RelayCommand]
    private async Task OpenTextFile()
    {
        IStorageFile? textFile = await fileService.OpenTextFile(null);
        if (textFile == null) return;

        FileName = textFile.Name;
        List<string> newFileLines = [];
        List<string> warnings = [];
        await using Stream stream = await textFile.OpenReadAsync();
        using StreamReader reader = new StreamReader(stream);
        while (await reader.ReadLineAsync() is { } line)
        {
            newFileLines.Add(line);
        }

        //If the file is a CSV we want to try getting a format string from the header
        if (newFileLines.Count > 0)
        {
            bool isCsv = Path.GetExtension(textFile.Name).Equals(".csv", StringComparison.OrdinalIgnoreCase);
            if (isCsv)
            {
                // Parse header to generate format string
                string? generatedFormat = ParseCsvHeader(newFileLines[0], out bool hasNoHeader);
                if (generatedFormat != null)
                {
                    FormatString = generatedFormat;
                }

                if (!hasNoHeader)
                {
                    newFileLines.RemoveAt(0);
                    if (ParseMessages.Count > 0 && ParseMessages.Any(m => m.IsError))
                    {
                        warnings.Add("CSV header will be ignored");
                        warnings.Add("");
                    }
                }
            }
        }

        //Add warnings if there is a mismatch between the number of lines in the file and the number of selected tracks
        if (newFileLines.Count < tracks.Count)
        {
            int skippedTracks = tracks.Count - newFileLines.Count;
            warnings.Add($"File has fewer lines than the number of selected tracks");
            string counter = skippedTracks == 1 ? "track" : "tracks";
            warnings.Add($"    {skippedTracks} {counter} will be skipped");
        }
        if (newFileLines.Count > tracks.Count)
        {
            int skippedLines = newFileLines.Count - tracks.Count;
            warnings.Add($"File has more lines than the number of selected tracks");
            string counter = skippedLines == 1 ? "line" : "lines";
            warnings.Add($"    {skippedLines} {counter} will be skipped");
        }

        FileWarnings = warnings;
        FileLines = newFileLines;
    }

    /// <summary>
    /// Parses CSV header and generates format string from column names. Returns null if there are any invalid column names.
    /// If it finds no valid column names, it assumes the header is data rather than a header and sets hasNoHeader to true.
    /// </summary>
    private string? ParseCsvHeader(string headerLine, out bool hasNoHeader)
    {
        string[] columns = headerLine.Split(',');
        List<(string column, bool isValid)> parsedColumns = [];
        List<Message> messages = [];

        foreach (string column in columns)
        {
            string trimmedColumn = column.Trim();
            PropertyInfo? property = PropertyHelpers.GetProperty<TrackViewModel>(trimmedColumn);
            if (property == null || !TrackProperties.Contains(property))
            {
                parsedColumns.Add((trimmedColumn, false));
                continue;
            }

            parsedColumns.Add((property.Name, true));
        }

        int validCount = parsedColumns.Count(col => col.isValid);
        int invalidCount = parsedColumns.Count - validCount;

        //If there are no valid columns, assume they're data rather than a header
        hasNoHeader = validCount == 0;

        //If there is a header and some columns were invalid, inform the user about which columns were valid and which were invalid
        if (invalidCount > 0 && !hasNoHeader)
        {
            messages.Add(new Message("Failed to parse CSV header:", true));
            foreach ((string column, bool isValid) in parsedColumns)
            {
                string validity = isValid ? "valid" : "invalid";
                messages.Add(new Message($"    Column \"{column}\" is {validity}", !isValid));
            }
        }

        //If there were no invalid columns, inform the user that the format was set from the header
        if (invalidCount == 0) messages.Add(new Message("Format set from CSV header", false));

        ParseMessages = messages;

        if (invalidCount > 0) return null;

        // Build format string with placeholders and comma delimiters
        return string.Join(",", parsedColumns.Select(col => $"{PlaceholderChar}{col.column}{PlaceholderChar}"));
    }

    /// <summary>
    /// Class to hold a message and whether it's an error.
    /// Needs to be a class rather than a tuple/struct because it's bound in an ItemsControl
    /// </summary>
    public class Message(string text, bool isError)
    {
        public string Text { get; set; } = text;
        public bool IsError { get; set; } = isError;
    }
}