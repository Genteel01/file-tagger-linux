using System;
using System.IO;
using ATL;
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

    public bool ArePicturesIdentical(PictureInfo pic1, PictureInfo pic2)
    {
        if(pic1.PictureHash == 0) pic1.ComputePicHash();
        if(pic2.PictureHash == 0) pic2.ComputePicHash();
        return pic1.PictureHash == pic2.PictureHash;
    }

    public bool ArePicturesIdentical(Bitmap pic1, Bitmap pic2)
    {
        if(pic1.Size != pic2.Size) return false;
        if(pic1.Format != pic2.Format) return false;
        if(pic1.PixelSize != pic2.PixelSize) return false;
        if(pic1.AlphaFormat != pic2.AlphaFormat) return false;
        if(pic1.Dpi != pic2.Dpi) return false;

        BitmapEncoderOptions picOptions = new PngBitmapEncoderOptions();
        using MemoryStream ms1 = new MemoryStream();
        using MemoryStream ms2 = new MemoryStream();
        pic1.Save(ms1, picOptions);
        pic2.Save(ms2, picOptions);

        return ArePicturesIdentical(ms1.GetBuffer(), ms2.GetBuffer());
    }

    private bool ArePicturesIdentical(byte[] pic1, byte[] pic2)
    {
        if(pic1.Length != pic2.Length) return false;

        for (int i = 0; i < pic1.Length; i++)
        {
            if (pic1[i] != pic2[i]) return false;
        }
        return true;
    }
}