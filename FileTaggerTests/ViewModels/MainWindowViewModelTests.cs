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
        IImageService imageService = new FakeImageService();
        EditPanelViewModel editPanelViewModel = new EditPanelViewModel(fileService, imageService);
        MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(fileService, preferenceService, editPanelViewModel);
        return mainWindowViewModel;
    }

    [Fact]
    public void OpenMusicFiles_FakeFileService_Runs()
    {
        MainWindowViewModel viewModel = CreateMockViewModel();

        viewModel.OpenMusicFilesCommand.Execute(CancellationToken.None);
    }


}