using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace FileTagger.Services;

public interface IFileService
{
    public Task<IStorageFile?> OpenFileAsync();
    public Task<IStorageFile?> SaveFileAsync();
    public Task<IReadOnlyList<IStorageFolder>> OpenFoldersAsync();
    public Task<(IReadOnlyList<IStorageFile>, bool)> OpenFilesRecursivelyAsync();
}