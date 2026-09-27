using ATL;
using FileTagger.ViewModels;

namespace FileTaggerTests.Fakers;

public static class MockCreators
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
}