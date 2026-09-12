using System.IO;
using ATL;
using Avalonia.Media.Imaging;

namespace FileTagger.Services;

public interface IImageService
{
    public Bitmap GetDefaultImage();
    public PictureInfo CreatePictureInfoFromBitmap(Bitmap bitmap, PictureInfo.PIC_TYPE pictureType);
    public PictureInfo CreatePictureInfoFromStream(Stream stream, PictureInfo.PIC_TYPE pictureType);
}