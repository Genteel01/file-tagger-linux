using FileTagger.Services;
using FileTagger.ViewModels;
using FileTaggerTests.Fakers;

namespace FileTaggerTests.ViewModels;

public class EditPanelViewModelTests
{
    private static EditPanelViewModel CreateMockViewModel()
    {
        IFileService fileService = new FakeFileService();
        IImageService imageService = new FakeImageService();
        EditPanelViewModel editPanelViewModel = new EditPanelViewModel(fileService, imageService);
        return editPanelViewModel;
    }

    [Fact]
    public void ReceiveMessage_NoTracks_NoError()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        MainWindowViewModel.SelectedItemsMessage message = new MainWindowViewModel.SelectedItemsMessage([]);

        viewModel.Receive(message);

        Assert.Empty(viewModel.SelectedTracks);
    }

    [Fact]
    public void ReceiveMessage_WithTracks_Assigned()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        List<TrackViewModel> stubTracks = [StubCreators.CreateStubTrackViewModel()];
        MainWindowViewModel.SelectedItemsMessage message = new MainWindowViewModel.SelectedItemsMessage(stubTracks);

        viewModel.Receive(message);

        Assert.Equal(stubTracks.Count, viewModel.SelectedTracks.Count);
        Assert.Equal(stubTracks[0], viewModel.SelectedTracks[0]);
        Assert.Equal(stubTracks, viewModel.SelectedTracks);
    }
}