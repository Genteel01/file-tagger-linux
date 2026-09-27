using ATL;
using FileTagger.ViewModels;

namespace FileTaggerTests.Fakers;

public static class TrackViewModelMocks
{
    public static TrackViewModel CreateMockTrackViewModel()
    {
        TrackViewModel viewModel = StubCreators.CreateStubTrackViewModel();
        viewModel.Title = "Title";
        viewModel.Album = "Album";
        viewModel.Artist = "Artist";
        viewModel.TrackNumber = 2;
        viewModel.DiscNumber = 3;
        viewModel.Year = 2024;
        viewModel.Genre = "Genre";
        viewModel.AlbumArtist = "Album artist";
        viewModel.Composer = "Composer";
        viewModel.Comment = "Comment";

        PictureInfo picture = new PictureInfo(PictureInfo.PIC_TYPE.Front)
        {
            Description = "Cover art"
        };
        viewModel.EmbeddedPictures.Add(picture);
        viewModel.Changed = false;
        return viewModel;
    }

    public static void AssertOriginalMockFields(TrackViewModel viewModel)
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
        PictureInfo copiedPicture = Assert.Single(viewModel.EmbeddedPictures);
        Assert.Equal(PictureInfo.PIC_TYPE.Front, copiedPicture.PicType);
        Assert.Equal("Cover art", copiedPicture.Description);
    }

    public static void AssertEmptyMockFields(TrackViewModel viewModel)
    {
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
}