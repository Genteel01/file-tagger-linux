using System;
using Avalonia.Styling;
using FileTagger.ViewModels;

namespace FileTagger.Statics;

public static class MyThemes
{

    public enum LayoutSize
    {
        Automatic,
        Standard,
        Compact,
    }

    public static readonly ThemeVariant LightGreen = new ThemeVariant(nameof(LightGreen), ThemeVariant.Light);
    public static readonly ThemeVariant DarkGreen = new ThemeVariant(nameof(DarkGreen), ThemeVariant.Dark);

    public static ThemeVariant StringToTheme(string themeName) => themeName switch
    {
        nameof(ThemeVariant.Light) => ThemeVariant.Light,
        nameof(ThemeVariant.Dark) => ThemeVariant.Dark,
        nameof(LightGreen) => LightGreen,
        nameof(DarkGreen) => DarkGreen,
        _ => ThemeVariant.Default
    };

    public static ThemeVariant GetOppositeTheme(ThemeVariant theme) => GetOppositeTheme(theme.ToString());

    private static ThemeVariant GetOppositeTheme(string themeName) => themeName switch
    {
        nameof(ThemeVariant.Light) => ThemeVariant.Dark,
        nameof(ThemeVariant.Dark) => ThemeVariant.Light,
        nameof(LightGreen) => DarkGreen,
        nameof(DarkGreen) => LightGreen,
        _ => ThemeVariant.Dark
    };
}

public static class Consts
{
    /// <summary>
    /// Value for edit fields that we don't want to change
    /// </summary>
    public const string UnchangedField = "< keep >";

    /// <summary>
    /// Name of the app, used in the window titlebar and the preferences save location
    /// </summary>
    public const string AppName = "NetTag";
}

public static class Sorts
{
    public const string PathSort = $"{nameof(TrackViewModel.Directory)}_{nameof(TrackViewModel.FileName)}";
    public const string TitleSort = $"{nameof(TrackViewModel.Title)}_{PathSort}";
    public const string AlbumSort = $"{nameof(TrackViewModel.Album)}_{nameof(TrackViewModel.DiscNumber)}_{nameof(TrackViewModel.TrackNumber)}_{TitleSort}";
    public const string ArtistSort = $"{nameof(TrackViewModel.Artist)}_{AlbumSort}";
    public const string AlbumArtistSort = $"{nameof(TrackViewModel.AlbumArtist)}_{AlbumSort}";
    public const string TrackNumberSort = $"{nameof(TrackViewModel.TrackNumber)}_{PathSort}";
    public const string DiscNumberSort = $"{nameof(TrackViewModel.DiscNumber)}_{TrackNumberSort}";
    public const string YearSort = $"{nameof(TrackViewModel.Year)}_{PathSort}";
    public const string GenreSort = $"{nameof(TrackViewModel.Genre)}_{PathSort}";
    public const string ComposerSort = $"{nameof(TrackViewModel.Composer)}_{PathSort}";
    public const string CommentSort = $"{nameof(TrackViewModel.Comment)}_{PathSort}";
    public const string DurationSort = $"{nameof(TrackViewModel.Duration)}_{PathSort}";
    public const string BitrateSort = $"{nameof(TrackViewModel.Bitrate)}_{PathSort}";
    public const string SampleRateSort = $"{nameof(TrackViewModel.SampleRate)}_{PathSort}";
    public const string AudioFormatSort = $"{nameof(TrackViewModel.AudioFormat)}_{PathSort}";

}

public static class Maths
{
    public static int ChangeCollectionIndex(int currentIndex, int collectionCount, int change)
    {
        int newIndex = currentIndex + change;
        if (newIndex >= collectionCount) return newIndex % collectionCount;
        if (newIndex < 0) return newIndex + collectionCount;
        return newIndex;
    }
}