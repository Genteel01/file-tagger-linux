using System.IO;
using ATL;
using ATL.AudioData;
using Avalonia.Media.Imaging;

namespace FileTagger.Services;

public class ImageService : IImageService
{
    public PictureInfo CreatePictureInfoFromBitmap(Bitmap bitmap, PictureInfo.PIC_TYPE pictureType)
    {
        BitmapEncoderOptions picOptions = new PngBitmapEncoderOptions();
        using MemoryStream ms = new MemoryStream();
        bitmap.Save(ms, picOptions);
        ms.Seek(0, SeekOrigin.Begin);
        return CreatePictureInfoFromStream(ms, pictureType);
    }

    public PictureInfo CreatePictureInfoFromStream(Stream stream, PictureInfo.PIC_TYPE pictureType)
    {
        PictureInfo picInfo = PictureInfo.fromBinaryData(stream, (int)stream.Length,
            pictureType, MetaDataIOFactory.TagType.ANY, 0);
        picInfo.ComputePicHash();
        return picInfo;
    }
}