using ATL;
using FileTagger.ViewModels;

namespace FileTaggerTests.ViewModels;

public class TrackViewModelTests
{
    private static Track CreateStubTrack()
    {
        Track track = new Track();
        return track;
    }

    [Fact]
    public void OnPropertyChanged_EditField_ChangedChanges()
    {
        Track track = CreateStubTrack();
        TrackViewModel viewModel =  new TrackViewModel(track);
        Assert.False(viewModel.Changed);
        viewModel.Title = "changed";
        Assert.True(viewModel.Changed);
    }

    [Fact]
    public void OnPropertyChanged_EditChanged_ChangedDoesntChange()
    {
        Track track = CreateStubTrack();
        TrackViewModel viewModel = new TrackViewModel(track)
        {
            Changed = true
        };
        Assert.True(viewModel.Changed);
        viewModel.Changed = false;
        Assert.False(viewModel.Changed);
    }
}