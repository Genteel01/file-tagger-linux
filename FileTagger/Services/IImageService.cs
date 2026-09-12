using ATL;
using Avalonia.Media.Imaging;

namespace FileTagger.Services;

public interface IImageService
{
    public Bitmap GetDefaultImage();
    public bool ArePicturesIdentical(Bitmap pic1, Bitmap pic2);
    public bool ArePicturesIdentical(PictureInfo pic1, PictureInfo pic2);
}