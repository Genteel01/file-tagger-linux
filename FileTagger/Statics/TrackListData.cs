using System.Collections.Generic;
using FileTagger.ViewModels;

namespace FileTagger.Statics;

public static class TrackListData
{
    public const string DummyPanelClass = "SplitterBuffer";
    public const string DummyPanelSizeGroup = "DummyPanel";
    public const string SplitterSizeGroupSuffix = "Splitter";
    public const double MinColumnWidth = 20;

    public static readonly List<(string name, string sort, string label)> PossibleListColumns = [
        (nameof(TrackViewModel.Title), TitleSort, "Title"),
        (nameof(TrackViewModel.Album), AlbumSort, "Album"),
        (nameof(TrackViewModel.Artist), ArtistSort, "Artist"),
        (nameof(TrackViewModel.AlbumArtist), AlbumArtistSort, "Album Artist"),
        (nameof(TrackViewModel.TrackNumber), TrackNumberSort, "Track"),
        (nameof(TrackViewModel.DiscNumber), DiscNumberSort, "Disc"),
        (nameof(TrackViewModel.Year), YearSort, "Year"),
        (nameof(TrackViewModel.Genre), GenreSort, "Genre"),
        (nameof(TrackViewModel.Composer), ComposerSort, "Composer"),
        (nameof(TrackViewModel.Comment), CommentSort, "Comment"),
        (nameof(TrackViewModel.Path), PathSort, "Path"),
        (nameof(TrackViewModel.CreatedDateFormatted), CreatedSort, "Created Date"),
        (nameof(TrackViewModel.ModifiedDateFormatted), ModifiedSort, "Modified Date"),
        (nameof(TrackViewModel.Duration), DurationSort, "Length"),
        (nameof(TrackViewModel.Bitrate), BitrateSort, "Bitrate"),
        (nameof(TrackViewModel.SampleRate), SampleRateSort, "Sample Rate"),
        (nameof(TrackViewModel.AudioFormat), AudioFormatSort, "Audio Format"),
    ];

    private const string PathSort = $"{nameof(TrackViewModel.Directory)}_{nameof(TrackViewModel.FileName)}";
    private const string TitleSort = $"{nameof(TrackViewModel.Title)}_{PathSort}";
    private const string AlbumSort = $"{nameof(TrackViewModel.Album)}_{nameof(TrackViewModel.DiscNumber)}_{nameof(TrackViewModel.TrackNumber)}_{TitleSort}";
    private const string ArtistSort = $"{nameof(TrackViewModel.Artist)}_{AlbumSort}";
    private const string AlbumArtistSort = $"{nameof(TrackViewModel.AlbumArtist)}_{AlbumSort}";
    private const string TrackNumberSort = $"{nameof(TrackViewModel.TrackNumber)}_{PathSort}";
    private const string DiscNumberSort = $"{nameof(TrackViewModel.DiscNumber)}_{TrackNumberSort}";
    private const string YearSort = $"{nameof(TrackViewModel.Year)}_{PathSort}";
    private const string GenreSort = $"{nameof(TrackViewModel.Genre)}_{PathSort}";
    private const string ComposerSort = $"{nameof(TrackViewModel.Composer)}_{PathSort}";
    private const string CommentSort = $"{nameof(TrackViewModel.Comment)}_{PathSort}";
    private const string DurationSort = $"{nameof(TrackViewModel.Duration)}_{PathSort}";
    private const string BitrateSort = $"{nameof(TrackViewModel.Bitrate)}_{PathSort}";
    private const string SampleRateSort = $"{nameof(TrackViewModel.SampleRate)}_{PathSort}";
    private const string AudioFormatSort = $"{nameof(TrackViewModel.AudioFormat)}_{PathSort}";
    private const string CreatedSort = $"{nameof(TrackViewModel.CreatedDate)}_{PathSort}";
    private const string ModifiedSort = $"{nameof(TrackViewModel.ModifiedDate)}_{PathSort}";

    public const string DefaultSort = PathSort;
}