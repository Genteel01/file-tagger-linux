using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Commons;

namespace FileTagger.Services;

public interface IFileService
{
    public Task<IReadOnlyList<IStorageFile>> OpenBookmarkedFilesAsync(List<string> extensions, string bookmarkId);
    public Task<(IReadOnlyList<IStorageFile> files, bool cancelled, string? bookmarkId)> OpenFilesRecursivelyAsync(List<string> extensions, string? bookmarkId = null);
    public Task<IReadOnlyList<IStorageFile>> OpenImageFiles();
    public Task<string?> SaveImageFile(Bitmap bitmap, ImageFormat format, string suggestedName, string? bookmarkId);
    public Task<T?> LoadObjectData<T>() where T : class?;
    public Task SaveJsonData(object data);
}