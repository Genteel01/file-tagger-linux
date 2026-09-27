using ATL;
using FileTagger.ViewModels;
using FileTaggerTests.Fakers;

namespace FileTaggerTests.ViewModels;

public class TrackViewModelTests
{
    [Fact]
    public void OnPropertyChanged_EditField_ChangedChanges()
    {
        TrackViewModel viewModel = MockCreators.CreateMockTrackViewModel();
        Assert.False(viewModel.Changed);
        viewModel.Title = "changed";
        Assert.True(viewModel.Changed);
    }

    [Fact]
    public void OnPropertyChanged_EditChanged_ChangedDoesntChange()
    {
        TrackViewModel viewModel = MockCreators.CreateMockTrackViewModel();
        viewModel.Changed = true;
        Assert.True(viewModel.Changed);
        viewModel.Changed = false;
        Assert.False(viewModel.Changed);
    }

    [Fact]
    public void RevertChanges_RestoresTagsAndPicturesFromTrack()
    {
        TrackViewModel viewModel = MockCreators.CreateMockTrackViewModel();
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
        TrackViewModel source = MockCreators.CreateMockTrackViewModel();
        TrackViewModel destination = StubCreators.CreateStubTrackViewModel();

        source.CopyTo(destination);

        AssertPopulatedTags(destination);
        PictureInfo copiedPicture = Assert.Single(destination.EmbeddedPictures);
        Assert.Equal(PictureInfo.PIC_TYPE.Front, copiedPicture.PicType);
        Assert.Equal("Cover art", copiedPicture.Description);
        Assert.NotSame(source.EmbeddedPictures[0], copiedPicture);
    }

    [Fact]
    public void ClearTags_ClearsAllTagsAndPictures()
    {
        TrackViewModel viewModel = MockCreators.CreateMockTrackViewModel();

        viewModel.ClearTags();

        Assert.Empty(viewModel.Title);
        Assert.Empty(viewModel.Album);
        Assert.Empty(viewModel.Artist);
        Assert.Null(viewModel.TrackNumber);
        Assert.Null(viewModel.DiscNumber);
        Assert.Null(viewModel.Year);
        Assert.Empty(viewModel.Genre);
        Assert.Empty(viewModel.AlbumArtist);
        Assert.Empty(viewModel.Composer);
        Assert.Empty(viewModel.Comment);
        Assert.Empty(viewModel.EmbeddedPictures);
        Assert.True(viewModel.Changed);
    }

    private static void AssertPopulatedTags(TrackViewModel viewModel)
    {
        Assert.Equal("Title", viewModel.Title);
        Assert.Equal("Album", viewModel.Album);
        Assert.Equal("Artist", viewModel.Artist);
        Assert.Equal(2, viewModel.TrackNumber);
        Assert.Equal(3, viewModel.DiscNumber);
        Assert.Equal(2024, viewModel.Year);
        Assert.Equal("Genre", viewModel.Genre);
        Assert.Equal("Album artist", viewModel.AlbumArtist);
        Assert.Equal("Composer", viewModel.Composer);
        Assert.Equal("Comment", viewModel.Comment);
    }
}