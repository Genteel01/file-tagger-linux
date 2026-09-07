using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace FileTagger.Services;


public class FileService(Window target) : IFileService
{
    private readonly string _folderPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify),
            "Genteel01.FileTagger");
    public async Task<(IReadOnlyList<IStorageFile>, bool)> OpenFilesRecursivelyAsync(List<string> extensions)
    {
        IReadOnlyList<IStorageFolder> folders = await target.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Open Folders",
            AllowMultiple = true,
        });

        List<IStorageFile> files = [];
        foreach (IStorageFolder folder in folders)
        {
            files.AddRange(await GetChildFiles(folder, extensions));
        }
        return (files, folders.Count == 0);
    }

    private async Task<IReadOnlyList<IStorageFile>> GetChildFiles(IStorageFolder folder, List<string> extensions)
    {
        IAsyncEnumerable<IStorageItem> items = folder.GetItemsAsync();

        List<IStorageFile> files = [];

        await foreach (IStorageItem item in items)
        {
            if (item is IStorageFile file)
            {
                string fileName = file.Name.ToLower();
                string extension = "." + fileName.Split(".").Last().ToLower();
                if (extensions.Contains(extension))
                {
                    files.Add(file);
                }
            }
            else if (item is IStorageFolder childFolder)
            {
                files.AddRange(await GetChildFiles(childFolder, extensions));
            }
        }

        return files;
    }

    public async Task<IReadOnlyList<IStorageFile>> OpenImageFiles()
    {
        IReadOnlyList<IStorageFile> files = await target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Folders",
            AllowMultiple = true,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });

        return files;
    }

    public async Task<T?> LoadObjectData<T>() where T : class?
    {
        try
        {
            string typeName = typeof(T).Name;
            string filePath = Path.Combine(_folderPath, $"{typeName}.txt");
            await using FileStream fs = File.OpenRead(filePath);
            T? loadedData = await JsonSerializer.DeserializeAsync<T>(fs);
            return loadedData;
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveJsonData(object data)
    {
        string typeName = data.GetType().Name;
        string filePath = Path.Combine(_folderPath, $"{typeName}.txt");
        // Ensure all directories exists
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        // We use a FileStream to write all items to disc
        await using FileStream fs = File.Create(filePath);
        await JsonSerializer.SerializeAsync(fs, data);
    }
}