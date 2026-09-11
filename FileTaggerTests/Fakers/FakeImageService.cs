using Avalonia.Media.Imaging;
using FileTagger.Services;

namespace FileTaggerTests.Fakers;

public class FakeImageService : IImageService
{
    public Bitmap GetDefaultImage()
    {
        return new Bitmap("");
    }
}