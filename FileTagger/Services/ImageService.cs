using System;
using System.IO;
using ATL;
using ATL.AudioData;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace FileTagger.Services;

public class ImageService : IImageService
{
    private Bitmap? _defaultImage;
    public Bitmap GetDefaultImage()
    {
        _defaultImage ??= new Bitmap(AssetLoader.Open(new Uri("avares://FileTagger/Assets/placeholder.png", UriKind.Absolute)));
        return _defaultImage;
    }

    public PictureInfo CreatePictureInfoFromBitmap(Bitmap bitmap, PictureInfo.PIC_TYPE pictureType)
    {
        BitmapEncoderOptions picOptions = new PngBitmapEncoderOptions();
        using MemoryStream ms = new MemoryStream();
        bitmap.Save(ms, picOptions);
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