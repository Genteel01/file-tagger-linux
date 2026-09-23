using ATL;
using Avalonia.Media.Imaging;
using FileTagger.Services;

namespace FileTaggerTests.Fakers;

public class FakeImageService : IImageService
{
    public PictureInfo CreatePictureInfoFromBitmap(Bitmap bitmap, PictureInfo.PIC_TYPE pictureType)
    {
        return new PictureInfo(PictureInfo.PIC_TYPE.Front);
    }

    public PictureInfo CreatePictureInfoFromStream(Stream stream, PictureInfo.PIC_TYPE pictureType)
    {
        return new PictureInfo(PictureInfo.PIC_TYPE.Front);
    }

    public Bitmap GetBitmap(PictureInfo picInfo)
    {
        using MemoryStream ms = new MemoryStream(picInfo.PictureData);
        return new Bitmap(ms);
    }
}