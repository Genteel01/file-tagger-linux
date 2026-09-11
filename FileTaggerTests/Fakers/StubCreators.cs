using ATL;
using FileTagger.ViewModels;

namespace FileTaggerTests.Fakers;

public static class StubCreators
{
    public static Track CreateStubTrack()
    {
        Track track = new Track();
        return track;
    }

    public static TrackViewModel CreateStubTrackViewModel()
    {
        Track track = CreateStubTrack();
        TrackViewModel trackViewModel = new TrackViewModel(track);
        return trackViewModel;
    }
}