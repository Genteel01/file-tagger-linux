using System.IO;
using ATL;
using Avalonia.Media.Imaging;

namespace FileTagger.Services;

public interface IImageService
{
    public PictureInfo CreatePictureInfoFromBitmap(Bitmap bitmap, PictureInfo.PIC_TYPE pictureType);
    public PictureInfo CreatePictureInfoFromStream(Stream stream, PictureInfo.PIC_TYPE pictureType);
    /// <summary>
    /// Gets a <see cref="Bitmap"/> of the given <see cref="PictureInfo"/>
    /// </summary>
    public Bitmap GetBitmap(PictureInfo picInfo);
}