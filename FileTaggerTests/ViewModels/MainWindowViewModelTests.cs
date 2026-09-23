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
        MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(fileService, preferenceService, editPanelViewModel);
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
        TrackViewModel firstTrack = StubCreators.CreateStubTrackViewModel();
        TrackViewModel secondTrack = StubCreators.CreateStubTrackViewModel();
        TrackViewModel thirdTrack = StubCreators.CreateStubTrackViewModel();
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
        TrackViewModel artistAAlbumZ = StubCreators.CreateStubTrackViewModel();
        TrackViewModel artistAAlbumA = StubCreators.CreateStubTrackViewModel();
        TrackViewModel artistBAlbumA = StubCreators.CreateStubTrackViewModel();
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
        TrackViewModel firstTrack = StubCreators.CreateStubTrackViewModel();
        TrackViewModel secondTrack = StubCreators.CreateStubTrackViewModel();
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
        TrackViewModel firstTrack = StubCreators.CreateStubTrackViewModel();
        TrackViewModel secondTrack = StubCreators.CreateStubTrackViewModel();
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
        TrackViewModel firstTrack = StubCreators.CreateStubTrackViewModel();
        TrackViewModel secondTrack = StubCreators.CreateStubTrackViewModel();
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

}