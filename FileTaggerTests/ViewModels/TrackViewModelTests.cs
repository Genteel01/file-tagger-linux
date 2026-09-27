using ATL;
using FileTagger.ViewModels;
using FileTaggerTests.Fakers;

namespace FileTaggerTests.ViewModels;

public class TrackViewModelTests
{
    [Fact]
    public void OnPropertyChanged_EditField_ChangedChanges()
    {
        TrackViewModel viewModel = TrackViewModelFakers.CreateMockTrackViewModel();
        Assert.False(viewModel.Changed);
        viewModel.Title = "changed";
        Assert.True(viewModel.Changed);
    }

    [Fact]
    public void OnPropertyChanged_EditChanged_ChangedDoesntChange()
    {
        TrackViewModel viewModel = TrackViewModelFakers.CreateMockTrackViewModel();
        viewModel.Changed = true;
        Assert.True(viewModel.Changed);
        viewModel.Changed = false;
        Assert.False(viewModel.Changed);
    }

    [Fact]
    public void RevertChanges_RestoresTagsAndPicturesFromTrack()
    {
        TrackViewModel viewModel = TrackViewModelFakers.CreateMockTrackViewModel();
        viewModel.RevertChanges();

        Assert.Null(viewModel.Title);
        Assert.Null(viewModel.Album);
        Assert.Null(viewModel.Artist);
        Assert.Equal(0, viewModel.TrackNumber);
        Assert.Null(viewModel.DiscNumber);
        Assert.Equal(0, viewModel.Year);
        Assert.Null(viewModel.Genre);
        Assert.Null(viewModel.AlbumArtist);
        Assert.Null(viewModel.Composer);
        Assert.Null(viewModel.Comment);
        Assert.False(viewModel.Changed);
        Assert.Empty(viewModel.EmbeddedPictures);
    }

    [Fact]
    public void CopyTo_CopiesAllEditableTagsAndPictures()
    {
        TrackViewModel source = TrackViewModelFakers.CreateMockTrackViewModel();
        TrackViewModel destination = TrackViewModelFakers.CreateStubTrackViewModel();

        source.CopyTo(destination);

        TrackViewModelFakers.AssertOriginalMockFields(destination);
        PictureInfo copiedPicture = destination.EmbeddedPictures[0];
        Assert.NotSame(source.EmbeddedPictures[0], copiedPicture);
    }

    [Fact]
    public void ClearTags_ClearsAllTagsAndPictures()
    {
        TrackViewModel viewModel = TrackViewModelFakers.CreateMockTrackViewModel();

        viewModel.ClearTags();

        TrackViewModelFakers.AssertEmptyMockFields(viewModel);
    }
}