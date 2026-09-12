using ATL;
using Avalonia.Media.Imaging;
using FileTagger.Services;

namespace FileTaggerTests.Fakers;

public class FakeImageService : IImageService
{
    public Bitmap GetDefaultImage()
    {
        return new Bitmap("");
    }

    public bool ArePicturesIdentical(Bitmap pic1, Bitmap pic2)
    {
        return false;
    }

    public bool ArePicturesIdentical(PictureInfo pic1, PictureInfo pic2)
    {
        return false;
    }
}