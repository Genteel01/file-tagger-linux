using ATL;
using FileTagger.Extensions;
using FileTagger.Services;
using FileTagger.ViewModels;
using FileTaggerTests.Extensions;
using FileTaggerTests.Fakers;

namespace FileTaggerTests.ViewModels;

public class EmbeddedPictureViewModelTests
{
    public static EmbeddedPictureViewModel CreateStubViewModel()
    {
        IFileService fileService = new FakeFileService();
        PreferenceService preferenceService = new PreferenceService(fileService);
        ImageService imageService = new ImageService();
        EmbeddedPictureViewModel stubEmbeddedPicture =
            new EmbeddedPictureViewModel(fileService, imageService, preferenceService, () => null);
        return stubEmbeddedPicture;
    }

    private static void AssertNoDisplayedPicture(EmbeddedPictureViewModel viewModel)
    {
        Assert.Null(viewModel.DisplayedPicture);
        Assert.False(viewModel.HasDisplayedPicture);
        Assert.False(viewModel.ShowImageNavigationButtons);
    }

    [Fact]
    public void Receive_NoTracks_ClearsDisplayedPictureState()
    {
        EmbeddedPictureViewModel viewModel = CreateStubViewModel();
        TrackViewModel track =
            TrackViewModelFakers.CreateMockTrackViewModelWithPictures(
                CreateFrontCoverPicture());
        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([track]));

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([]));

        Assert.False(viewModel.SelectedTrackHasPictures);
        Assert.Empty(viewModel.SelectedTracks);
        AssertNoDisplayedPicture(viewModel);
    }

    [Fact]
    public void Receive_SingleTrack_DisplaysOnlyPicturesOfSelectedType()
    {
        EmbeddedPictureViewModel viewModel = CreateStubViewModel();
        PictureInfo frontCover = CreateFrontCoverPicture();
        PictureInfo backCover = CreateBackCoverPicture();
        PictureInfo alternateFrontCover = CreateAlternateFrontCoverPicture();
        TrackViewModel track = TrackViewModelFakers.CreateMockTrackViewModelWithPictures(
            frontCover,
            backCover,
            alternateFrontCover);

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([track]));

        Assert.Equal(
            [frontCover, alternateFrontCover],
            track.EmbeddedPictures
                .Where(picture => picture.PicType == PictureInfo.PIC_TYPE.Front));
        Assert.Same(frontCover, viewModel.DisplayedPicture);
        Assert.True(viewModel.DisplayedPicture?.TrueEqual(frontCover));
        Assert.True(viewModel.SelectedTrackHasPictures);
        Assert.True(viewModel.HasDisplayedPicture);
        Assert.True(viewModel.ShowImageNavigationButtons);
    }

    [Fact]
    public void Receive_MultipleTracksWithMatchingPictures_DisplaysSharedPictures()
    {
        EmbeddedPictureViewModel viewModel = CreateStubViewModel();
        PictureInfo firstTrackFrontCover = CreateFrontCoverPicture();
        PictureInfo secondTrackFrontCover = CreateFrontCoverPicture();
        TrackViewModel firstTrack = TrackViewModelFakers.CreateMockTrackViewModelWithPictures(
            firstTrackFrontCover,
            CreateAlternateFrontCoverPicture());
        TrackViewModel secondTrack = TrackViewModelFakers.CreateMockTrackViewModelWithPictures(
            secondTrackFrontCover,
            CreateAlternateFrontCoverPicture());

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([firstTrack, secondTrack]));

        Assert.Equal(2, firstTrack.EmbeddedPictures.Count);
        Assert.True(viewModel.SelectedTrackHasPictures);
        Assert.True(viewModel.HasDisplayedPicture);
        Assert.True(viewModel.ShowImageNavigationButtons);
        //Displays the first track's picture by reference, but should only match equality with the second track's picture
        Assert.True(viewModel.DisplayedPicture?.TrueEqual(firstTrackFrontCover));
        Assert.True(viewModel.DisplayedPicture?.TrueEqual(secondTrackFrontCover));
        Assert.Same(firstTrackFrontCover, viewModel.DisplayedPicture);
        Assert.NotSame(secondTrackFrontCover, viewModel.DisplayedPicture);
    }

    [Fact]
    public void Receive_MultipleTracksWithDifferentPictures_HidesDisplayedPicture()
    {
        EmbeddedPictureViewModel viewModel = CreateStubViewModel();
        TrackViewModel firstTrack =
            TrackViewModelFakers.CreateMockTrackViewModelWithPictures(
                CreateFrontCoverPicture());
        TrackViewModel secondTrack = TrackViewModelFakers.CreateStubTrackViewModel();

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([firstTrack, secondTrack]));

        Assert.True(viewModel.SelectedTrackHasPictures);
        AssertNoDisplayedPicture(viewModel);
    }

    [Fact]
    public void Receive_ChangingPictureType_DisplaysPicturesOfNewType()
    {
        EmbeddedPictureViewModel viewModel = CreateStubViewModel();
        PictureInfo backCover = CreateBackCoverPicture();
        TrackViewModel track = TrackViewModelFakers.CreateMockTrackViewModelWithPictures(
            CreateFrontCoverPicture(),
            backCover);
        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([track]));

        viewModel.SelectedPictureType = PictureInfo.PIC_TYPE.Back;

        Assert.Same(backCover, viewModel.DisplayedPicture);
        Assert.True(viewModel.DisplayedPicture?.TrueEqual(backCover));
        Assert.True(viewModel.SelectedTrackHasPictures);
    }

    private static PictureInfo CreateFrontCoverPicture()
    {
        return PictureInfoTests.CreateMockPicture(PictureInfo.PIC_TYPE.Front, "Front cover");
    }

    private static PictureInfo CreateBackCoverPicture()
    {
        return PictureInfoTests.CreateMockPicture(PictureInfo.PIC_TYPE.Back, "Back cover");
    }

    private static PictureInfo CreateAlternateFrontCoverPicture()
    {
        return PictureInfoTests.CreateMockPicture(PictureInfo.PIC_TYPE.Front, "Alternate front");
    }
}