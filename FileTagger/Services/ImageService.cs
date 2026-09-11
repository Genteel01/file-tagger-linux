using System;
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
}