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

    public async Task<(IReadOnlyList<IStorageFile>, bool)> OpenFilesRecursivelyAsync()
    {
        IReadOnlyList<IStorageFolder> folders = await _target.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Open Folders",
            AllowMultiple = true,
        });

        List<IStorageFile> files = [];
        foreach (var folder in folders)
        {
            files.AddRange(await GetChildFiles(folder));
        }
        return (files, folders.Count == 0);
    }

    private async Task<IReadOnlyList<IStorageFile>> GetChildFiles(IStorageFolder folder)
    {
        IAsyncEnumerable<IStorageItem> items = folder.GetItemsAsync();

        List<IStorageFile> files = [];

        await foreach (var item in items)
        {
            if (item is IStorageFile file)
            {
                files.Add(file);
            }
            else if (item is IStorageFolder childFolder)
            {
                files.AddRange(await GetChildFiles(childFolder));
            }
        }

        return files;
    }
}