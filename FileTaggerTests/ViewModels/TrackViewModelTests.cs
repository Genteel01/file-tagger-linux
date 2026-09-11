using ATL;
using FileTagger.ViewModels;
using FileTaggerTests.Fakers;

namespace FileTaggerTests.ViewModels;

public class TrackViewModelTests
{
    [Fact]
    public void OnPropertyChanged_EditField_ChangedChanges()
    {
        Track track = StubCreators.CreateStubTrack();
        TrackViewModel viewModel =  new TrackViewModel(track);
        Assert.False(viewModel.Changed);
        viewModel.Title = "changed";
        Assert.True(viewModel.Changed);
    }

    [Fact]
    public void OnPropertyChanged_EditChanged_ChangedDoesntChange()
    {
        Track track = StubCreators.CreateStubTrack();
        TrackViewModel viewModel = new TrackViewModel(track)
        {
            Changed = true
        };
        Assert.True(viewModel.Changed);
        viewModel.Changed = false;
        Assert.False(viewModel.Changed);
    }
}