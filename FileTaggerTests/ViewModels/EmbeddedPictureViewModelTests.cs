using FileTagger.Services;
using FileTagger.ViewModels;
using FileTaggerTests.Fakers;

namespace FileTaggerTests.ViewModels;

public class EmbeddedPictureViewModelTests
{
    public static EmbeddedPictureViewModel CreateStubViewModel()
    {
        IFileService fileService = new FakeFileService();
        PreferenceService preferenceService = new PreferenceService(fileService);
        ImageService imageService = new ImageService();
        EmbeddedPictureViewModel stubEmbeddedPicture = new EmbeddedPictureViewModel(fileService, imageService, preferenceService, () => null);
        return stubEmbeddedPicture;
    }
}