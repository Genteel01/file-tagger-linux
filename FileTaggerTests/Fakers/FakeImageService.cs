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

    public PictureInfo CreatePictureInfoFromBitmap(Bitmap bitmap, PictureInfo.PIC_TYPE pictureType)
    {
        return new PictureInfo(PictureInfo.PIC_TYPE.Front);
    }

    public PictureInfo CreatePictureInfoFromStream(Stream stream, PictureInfo.PIC_TYPE pictureType)
    {
        return new PictureInfo(PictureInfo.PIC_TYPE.Front);
    }
}