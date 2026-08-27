using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace FileTagger.Services;

public interface IFileService
{
    public Task<(IReadOnlyList<IStorageFile>, bool)> OpenFilesRecursivelyAsync();
    public Task<IReadOnlyList<IStorageFile>> OpenImageFiles();
}