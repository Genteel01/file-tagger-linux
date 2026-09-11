using Avalonia.Media.Imaging;

namespace FileTagger.Services;

public interface IImageService
{
    public Bitmap GetDefaultImage();
}