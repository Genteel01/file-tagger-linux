using System.Collections.Generic;
using Avalonia.Data;
using FileTagger.ViewModels;

namespace FileTagger.Statics;

public static class TrackListData
{
    public const string DummyPanelClass = "SplitterBuffer";
    public const string DummyPanelSizeGroup = "DummyPanel";
    public const string SplitterSizeGroupSuffix = "Splitter";
    public const double MinColumnWidth = 20;

    public const string DefaultSort = PathSort;

    private const double DefaultWideWidth = 200;
    private const double DefaultStandardWidth = 100;
    private const double DefaultNarrowWidth = 60;

    public class ListColumnData(string sort, string label, CompiledBinding propertyBinding, double defaultWidth, bool defaultVisible = true)
    {
        public readonly string Sort = sort;
        public readonly string Label = label;
        public readonly CompiledBinding PropertyBinding = propertyBinding;
        public readonly bool DefaultVisible = defaultVisible;
        public readonly double DefaultWidth = defaultWidth;
    }

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

    public static readonly Dictionary<string, ListColumnData> PossibleListColumns = new Dictionary<string, ListColumnData> {
        { nameof(TrackViewModel.Title), new ListColumnData(TitleSort, "Title", CompiledBinding.Create<TrackViewModel, string>(track => track.Title), DefaultWideWidth) },
        { nameof(TrackViewModel.Album), new ListColumnData(AlbumSort, "Album", CompiledBinding.Create<TrackViewModel, string>(track => track.Album), DefaultWideWidth) },
        { nameof(TrackViewModel.Artist), new ListColumnData(ArtistSort, "Artist", CompiledBinding.Create<TrackViewModel, string>(track => track.Artist), DefaultWideWidth) },
        { nameof(TrackViewModel.AlbumArtist), new ListColumnData(AlbumArtistSort, "Album Artist", CompiledBinding.Create<TrackViewModel, string>(track => track.AlbumArtist), DefaultWideWidth) },
        { nameof(TrackViewModel.TrackNumber), new ListColumnData(TrackNumberSort, "Track", CompiledBinding.Create<TrackViewModel, int?>(track => track.TrackNumber), DefaultNarrowWidth) },
        { nameof(TrackViewModel.DiscNumber), new ListColumnData(DiscNumberSort, "Disc", CompiledBinding.Create<TrackViewModel, int?>(track => track.DiscNumber), DefaultNarrowWidth) },
        { nameof(TrackViewModel.Year), new ListColumnData(YearSort, "Year", CompiledBinding.Create<TrackViewModel, int?>(track => track.Year), DefaultNarrowWidth) },
        { nameof(TrackViewModel.Genre), new ListColumnData(GenreSort, "Genre", CompiledBinding.Create<TrackViewModel, string>(track => track.Genre), DefaultStandardWidth) },
        { nameof(TrackViewModel.Composer), new ListColumnData(ComposerSort, "Composer", CompiledBinding.Create<TrackViewModel, string>(track => track.Composer), DefaultWideWidth) },
        { nameof(TrackViewModel.Comment), new ListColumnData(CommentSort, "Comment", CompiledBinding.Create<TrackViewModel, string>(track => track.Comment), DefaultWideWidth) },
        { nameof(TrackViewModel.Path), new ListColumnData(PathSort, "Path", CompiledBinding.Create<TrackViewModel, string>(track => track.Path), DefaultWideWidth) },
        { nameof(TrackViewModel.CreatedDateFormatted), new ListColumnData(CreatedSort, "Created Date", CompiledBinding.Create<TrackViewModel, string?>(track => track.CreatedDateFormatted), DefaultWideWidth, false) },
        { nameof(TrackViewModel.ModifiedDateFormatted), new ListColumnData(ModifiedSort, "Modified Date", CompiledBinding.Create<TrackViewModel, string?>(track => track.ModifiedDateFormatted), DefaultWideWidth) },
        { nameof(TrackViewModel.Duration), new ListColumnData(DurationSort, "Length", CompiledBinding.Create<TrackViewModel, string>(track => track.Duration), DefaultStandardWidth) },
        { nameof(TrackViewModel.Bitrate), new ListColumnData(BitrateSort, "Bitrate", CompiledBinding.Create<TrackViewModel, string>(track => track.Bitrate), DefaultStandardWidth, false) },
        { nameof(TrackViewModel.SampleRate), new ListColumnData(SampleRateSort, "Sample Rate", CompiledBinding.Create<TrackViewModel, string>(track => track.SampleRate), DefaultStandardWidth, false) },
        { nameof(TrackViewModel.AudioFormat), new ListColumnData(AudioFormatSort, "Audio Format", CompiledBinding.Create<TrackViewModel, string>(track => track.AudioFormat), DefaultWideWidth, false) },
    };
}