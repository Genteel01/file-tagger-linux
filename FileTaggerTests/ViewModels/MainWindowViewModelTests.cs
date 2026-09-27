using FileTagger.Services;
using FileTagger.ViewModels;
using FileTaggerTests.Fakers;

namespace FileTaggerTests.ViewModels;

public class MainWindowViewModelTests
{
    private static MainWindowViewModel CreateMockViewModel()
    {
        IFileService fileService = new FakeFileService();
        PreferenceService preferenceService = new PreferenceService(fileService);
        EditPanelViewModel editPanelViewModel = EditPanelViewModelTests.CreateMockViewModel();
        MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(fileService, preferenceService, editPanelViewModel, () => null);
        return mainWindowViewModel;
    }

    [Fact]
    public void OpenMusicFiles_FakeFileService_Runs()
    {
        MainWindowViewModel viewModel = CreateMockViewModel();

        viewModel.OpenMusicFilesCommand.Execute(CancellationToken.None);
    }

    [Fact]
    public void SortTracks_SingleField_SortsTracksCorrectly()
    {
        MainWindowViewModel viewModel = CreateMockViewModel();
        TrackViewModel firstTrack = TrackViewModelFakers.CreateStubTrackViewModel();
        TrackViewModel secondTrack = TrackViewModelFakers.CreateStubTrackViewModel();
        TrackViewModel thirdTrack = TrackViewModelFakers.CreateStubTrackViewModel();
        const string sortField = nameof(TrackViewModel.Artist);
        firstTrack.Artist = "B";
        secondTrack.Artist = "A";
        thirdTrack.Artist = "C";
        viewModel.Tracks = [firstTrack, secondTrack, thirdTrack];

        viewModel.SortTracks(sortField, false);

        Assert.Equal([secondTrack, firstTrack, thirdTrack], viewModel.Tracks);
    }

    [Fact]
    public void SortTracks_MultipleFields_SortsTracksInSequence()
    {
        MainWindowViewModel viewModel = CreateMockViewModel();
        TrackViewModel artistAAlbumZ = TrackViewModelFakers.CreateStubTrackViewModel();
        TrackViewModel artistAAlbumA = TrackViewModelFakers.CreateStubTrackViewModel();
        TrackViewModel artistBAlbumA = TrackViewModelFakers.CreateStubTrackViewModel();
        const string sortFields = $"{nameof(TrackViewModel.Artist)}_{nameof(TrackViewModel.Album)}";
        artistAAlbumZ.Artist = "A";
        artistAAlbumZ.Album = "Z";
        artistAAlbumA.Artist = "A";
        artistAAlbumA.Album = "A";
        artistBAlbumA.Artist = "B";
        artistBAlbumA.Album = "A";
        viewModel.Tracks = [artistAAlbumZ, artistBAlbumA, artistAAlbumA];

        viewModel.SortTracks(sortFields, false);

        Assert.Equal([artistAAlbumA, artistAAlbumZ, artistBAlbumA], viewModel.Tracks);
    }

    [Fact]
    public void SortTracks_SwapDirectionFalse_DoesNotChangeDirection()
    {
        MainWindowViewModel viewModel = CreateMockViewModel();
        TrackViewModel firstTrack = TrackViewModelFakers.CreateStubTrackViewModel();
        TrackViewModel secondTrack = TrackViewModelFakers.CreateStubTrackViewModel();
        firstTrack.Artist = "A";
        secondTrack.Artist = "B";
        viewModel.Tracks = [firstTrack, secondTrack];
        const string sortField = nameof(TrackViewModel.Artist);
        viewModel.SortTracks(sortField, false);
        viewModel.SortTracks(sortField, true);

        viewModel.SortTracks(sortField, false);

        Assert.True(viewModel.SortDescending);
        Assert.Equal([secondTrack, firstTrack], viewModel.Tracks);
    }

    [Fact]
    public void SortTracks_SameFieldTwiceWithSwapDirection_SwapsDirection()
    {
        MainWindowViewModel viewModel = CreateMockViewModel();
        TrackViewModel firstTrack = TrackViewModelFakers.CreateStubTrackViewModel();
        TrackViewModel secondTrack = TrackViewModelFakers.CreateStubTrackViewModel();
        firstTrack.Artist = "A";
        secondTrack.Artist = "B";
        viewModel.Tracks = [firstTrack, secondTrack];
        const string sortField = nameof(TrackViewModel.Artist);

        viewModel.SortTracks(sortField, false);
        viewModel.SortTracks(sortField, true);

        Assert.True(viewModel.SortDescending);
        Assert.Equal([secondTrack, firstTrack], viewModel.Tracks);
    }

    [Fact]
    public void SortTracks_NewFieldWithSwapDirection_ResetsSortDescending()
    {
        MainWindowViewModel viewModel = CreateMockViewModel();
        TrackViewModel firstTrack = TrackViewModelFakers.CreateStubTrackViewModel();
        TrackViewModel secondTrack = TrackViewModelFakers.CreateStubTrackViewModel();
        firstTrack.Artist = "A";
        firstTrack.Album = "B";
        secondTrack.Artist = "B";
        secondTrack.Album = "A";
        viewModel.Tracks = [firstTrack, secondTrack];
        const string firstSortField = nameof(TrackViewModel.Artist);
        const string secondSortField = nameof(TrackViewModel.Album);
        viewModel.SortTracks(firstSortField, false);
        viewModel.SortTracks(firstSortField, true);

        viewModel.SortTracks(secondSortField, true);

        Assert.False(viewModel.SortDescending);
        Assert.Equal([secondTrack, firstTrack], viewModel.Tracks);
    }

    [Fact]
    public void CalculateChangeGroups_MarksBoundariesOfConsecutiveChangedTracks()
    {
        MainWindowViewModel viewModel = CreateMockViewModel();
        TrackViewModel[] tracks = Enumerable.Range(0, 5)
            .Select(index => TrackViewModelFakers.CreateStubTrackViewModel())
            .ToArray();
        tracks[0].Artist = "A";
        tracks[1].Artist = "B";
        tracks[2].Artist = "C";
        tracks[3].Artist = "D";
        tracks[4].Artist = "E";
        foreach (TrackViewModel track in tracks)
        {
            track.Changed = false;
        }
        tracks[1].Changed = true;
        tracks[2].Changed = true;
        tracks[4].Changed = true;
        viewModel.Tracks = tracks.ToList();

        viewModel.SortTracks(nameof(TrackViewModel.Artist), false);

        Assert.Equal(
            [(false, false), (true, false), (false, true), (false, false), (true, true)],
            viewModel.Tracks.Select(track => (track.IsChangeStart, track.IsChangeEnd)));
    }

    [Fact]
    public void CalculateChangeGroups_ClearsBoundariesWhenNoTracksHaveChanges()
    {
        MainWindowViewModel viewModel = CreateMockViewModel();
        TrackViewModel firstTrack = TrackViewModelFakers.CreateStubTrackViewModel();
        TrackViewModel secondTrack = TrackViewModelFakers.CreateStubTrackViewModel();
        firstTrack.Artist = "A";
        secondTrack.Artist = "B";
        secondTrack.Changed = false;
        firstTrack.Changed = true;
        viewModel.Tracks = [firstTrack, secondTrack];

        viewModel.SortTracks(nameof(TrackViewModel.Artist), false);
        firstTrack.Changed = false;
        viewModel.SortTracks(nameof(TrackViewModel.Artist), false);

        Assert.All(viewModel.Tracks, track =>
        {
            Assert.False(track.IsChangeStart);
            Assert.False(track.IsChangeEnd);
        });
    }

    [Fact]
    public void CutTags_PasteTags_CopiesTagsThenClearsCutTrack()
    {
        MainWindowViewModel viewModel = CreateMockViewModel();
        TrackViewModel cutTrack = TrackViewModelFakers.CreateMockTrackViewModel();
        TrackViewModel destinationTrack = TrackViewModelFakers.CreateStubTrackViewModel();
        viewModel.Tracks = [cutTrack, destinationTrack];
        viewModel.SelectedTracks.Add(cutTrack);
        viewModel.SelectionChanged();

        viewModel.CutTagsCommand.Execute(null);

        Assert.True(cutTrack.IsCutting);
        TrackViewModelFakers.AssertOriginalMockFields(cutTrack);

        viewModel.SelectedTracks.Clear();
        viewModel.SelectedTracks.Add(destinationTrack);
        viewModel.SelectionChanged();

        Assert.True(viewModel.PasteTagsCommand.CanExecute(null));

        viewModel.PasteTagsCommand.Execute(null);

        TrackViewModelFakers.AssertOriginalMockFields(destinationTrack);
        TrackViewModelFakers.AssertEmptyMockFields(cutTrack);
        Assert.False(cutTrack.IsCutting);
    }

    [Fact]
    public void CutTags_CutAnotherTrack_CancelsFirstCutMarkerWithoutClearing()
    {
        MainWindowViewModel viewModel = CreateMockViewModel();
        TrackViewModel firstTrack = TrackViewModelFakers.CreateMockTrackViewModel();
        TrackViewModel secondTrack = TrackViewModelFakers.CreateMockTrackViewModel();
        viewModel.Tracks = [firstTrack, secondTrack];
        viewModel.SelectedTracks.Add(firstTrack);
        viewModel.SelectionChanged();

        viewModel.CutTagsCommand.Execute(null);
        viewModel.SelectedTracks.Clear();
        viewModel.SelectedTracks.Add(secondTrack);
        viewModel.SelectionChanged();

        viewModel.CutTagsCommand.Execute(null);

        Assert.False(firstTrack.IsCutting);
        Assert.True(secondTrack.IsCutting);
        TrackViewModelFakers.AssertOriginalMockFields(firstTrack);
    }

    [Fact]
    public void CopyTags_WhileCutPending_CancelsCutWithoutClearingOriginal()
    {
        MainWindowViewModel viewModel = CreateMockViewModel();
        TrackViewModel cutTrack = TrackViewModelFakers.CreateMockTrackViewModel();
        TrackViewModel copiedTrack = TrackViewModelFakers.CreateStubTrackViewModel();
        viewModel.Tracks = [cutTrack, copiedTrack];
        viewModel.SelectedTracks.Add(cutTrack);
        viewModel.SelectionChanged();
        viewModel.CutTagsCommand.Execute(null);
        viewModel.SelectedTracks.Clear();
        viewModel.SelectedTracks.Add(copiedTrack);
        viewModel.SelectionChanged();

        viewModel.CopyTagsCommand.Execute(null);

        Assert.False(cutTrack.IsCutting);
        TrackViewModelFakers.AssertOriginalMockFields(cutTrack);
    }
}