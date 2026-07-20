using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace FileTagger.Services;

public interface IFileService
{
    public Task<IStorageFile?> OpenFileAsync();
    public Task<IStorageFile?> SaveFileAsync();
}