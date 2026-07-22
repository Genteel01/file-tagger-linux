using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace FileTagger.Services;


public class FileService : IFileService
{
    private readonly Window _target;

    public FileService(Window target)
    {
        _target = target;
    }
    public async Task<IStorageFile?> OpenFileAsync()
    {
        var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open Text File",
            AllowMultiple = false,
        });

        return files.Count >= 1 ? files[0] : null;
    }

    public async Task<IReadOnlyList<IStorageFolder>> OpenFoldersAsync()
    {

        var folders = await _target.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Open Folders",
            AllowMultiple = true,
        });

        return folders;
    }

    public async Task<IStorageFile?> SaveFileAsync()
    {
        return await _target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save Text File"
        });
    }
}