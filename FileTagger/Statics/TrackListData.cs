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

    public struct ListColumnData
    {
        public string Sort;
        public string Label;
        public CompiledBinding PropertyBinding;
    }

    public static readonly Dictionary<string, ListColumnData> PossibleListColumns = new Dictionary<string, ListColumnData> {
        { nameof(TrackViewModel.Title), new ListColumnData { Sort = TitleSort, Label = "Title", PropertyBinding = CompiledBinding.Create<TrackViewModel, string>(track => track.Title) } },
        { nameof(TrackViewModel.Album), new ListColumnData { Sort = AlbumSort, Label = "Album", PropertyBinding = CompiledBinding.Create<TrackViewModel, string>(track => track.Album) } },
        { nameof(TrackViewModel.Artist), new ListColumnData { Sort = ArtistSort, Label = "Artist", PropertyBinding = CompiledBinding.Create<TrackViewModel, string>(track => track.Artist) } },
        { nameof(TrackViewModel.AlbumArtist), new ListColumnData { Sort = AlbumArtistSort, Label = "Album Artist", PropertyBinding = CompiledBinding.Create<TrackViewModel, string>(track => track.AlbumArtist) } },
        { nameof(TrackViewModel.TrackNumber), new ListColumnData { Sort = TrackNumberSort, Label = "Track", PropertyBinding = CompiledBinding.Create<TrackViewModel, int?>(track => track.TrackNumber) } },
        { nameof(TrackViewModel.DiscNumber), new ListColumnData { Sort = DiscNumberSort, Label = "Disc", PropertyBinding = CompiledBinding.Create<TrackViewModel, int?>(track => track.DiscNumber) } },
        { nameof(TrackViewModel.Year), new ListColumnData { Sort = YearSort, Label = "Year", PropertyBinding = CompiledBinding.Create<TrackViewModel, int?>(track => track.Year) } },
        { nameof(TrackViewModel.Genre), new ListColumnData { Sort = GenreSort, Label = "Genre", PropertyBinding = CompiledBinding.Create<TrackViewModel, string>(track => track.Genre) } },
        { nameof(TrackViewModel.Composer), new ListColumnData { Sort = ComposerSort, Label = "Composer", PropertyBinding = CompiledBinding.Create<TrackViewModel, string>(track => track.Composer) } },
        { nameof(TrackViewModel.Comment), new ListColumnData { Sort = CommentSort, Label = "Comment", PropertyBinding = CompiledBinding.Create<TrackViewModel, string>(track => track.Comment) } },
        { nameof(TrackViewModel.Path), new ListColumnData { Sort = PathSort, Label = "Path", PropertyBinding = CompiledBinding.Create<TrackViewModel, string>(track => track.Path) } },
        { nameof(TrackViewModel.CreatedDateFormatted), new ListColumnData { Sort = CreatedSort, Label = "Created Date", PropertyBinding = CompiledBinding.Create<TrackViewModel, string?>(track => track.CreatedDateFormatted) } },
        { nameof(TrackViewModel.ModifiedDateFormatted), new ListColumnData { Sort = ModifiedSort, Label = "Modified Date", PropertyBinding = CompiledBinding.Create<TrackViewModel, string?>(track => track.ModifiedDateFormatted) } },
        { nameof(TrackViewModel.Duration), new ListColumnData { Sort = DurationSort, Label = "Length", PropertyBinding = CompiledBinding.Create<TrackViewModel, string>(track => track.Duration) } },
        { nameof(TrackViewModel.Bitrate), new ListColumnData { Sort = BitrateSort, Label = "Bitrate", PropertyBinding = CompiledBinding.Create<TrackViewModel, string>(track => track.Bitrate) } },
        { nameof(TrackViewModel.SampleRate), new ListColumnData { Sort = SampleRateSort, Label = "Sample Rate", PropertyBinding = CompiledBinding.Create<TrackViewModel, string>(track => track.SampleRate) } },
        { nameof(TrackViewModel.AudioFormat), new ListColumnData { Sort = AudioFormatSort, Label = "Audio Format", PropertyBinding = CompiledBinding.Create<TrackViewModel, string>(track => track.AudioFormat) } },
    };

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