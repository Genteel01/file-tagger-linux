using System.Collections.Generic;
using System.IO;
using ATL;
using ATL.AudioData;
using Avalonia.Media.Imaging;

namespace FileTagger.Services;

public class ImageService : IImageService
{
    /// <summary>
    /// Dictionary of Bitmaps mapped to the corresponding <see cref="PictureInfo.PictureHash"/>,
    /// so we don't have to re-decode the same image multiple times
    /// </summary>
    private readonly Dictionary<uint, Bitmap> _cachedImages = new Dictionary<uint, Bitmap>();

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

    /// <summary>
    /// Gets a <see cref="Bitmap"/> of the given <see cref="PictureInfo"/>
    /// </summary>
    public Bitmap GetBitmap(PictureInfo picInfo)
    {
        _cachedImages.TryGetValue(picInfo.PictureHash, out Bitmap? bitmap);
        if (bitmap == null)
        {
            bitmap = new Bitmap(new MemoryStream(picInfo.PictureData));
            _cachedImages[picInfo.PictureHash] = bitmap;
        }
        return bitmap;
    }
}