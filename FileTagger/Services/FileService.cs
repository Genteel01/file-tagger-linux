using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Commons;

namespace FileTagger.Services;


public class FileService(Func<TopLevel?> getTarget) : IFileService
{
    private readonly string _folderPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify),
            "Genteel01.FileTagger");

    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { IncludeFields = true, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals };

    public async Task<IReadOnlyList<IStorageFile>> OpenBookmarkedFilesAsync(List<string> extensions, string bookmarkId)
    {
        TopLevel? target = getTarget();
        if (target == null) return [];
        IStorageBookmarkFolder? initialLocation = await target.StorageProvider.OpenFolderBookmarkAsync(bookmarkId);
        if (initialLocation == null) return [];

        bool showHiddenFiles = initialLocation.Name.StartsWith('.');
        IReadOnlyList<IStorageFile> files = await GetChildFiles(initialLocation, extensions, showHiddenFiles);
        return files;
    }

    public async Task<(IReadOnlyList<IStorageFile>, bool, string?)> OpenFilesRecursivelyAsync(List<string> extensions, string? bookmarkId = null)
    {
        TopLevel? target = getTarget();
        if (target == null) return ([], true, null);
        //Load initial location from bookmark
        IStorageBookmarkFolder? bookmarkFolder = null;
        if (bookmarkId != null) bookmarkFolder = await target.StorageProvider.OpenFolderBookmarkAsync(bookmarkId);

        //Get music folder if there is no bookmark
        IStorageFolder? initialLocation = bookmarkFolder ?? await target.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Music);

        IReadOnlyList<IStorageFolder> folders = await target.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Open Folders",
            AllowMultiple = false,
            SuggestedStartLocation = initialLocation,
        });

        if (folders.Count == 0) return ([], true, null);

        IStorageFolder folder = folders[0];
        bool showHiddenFiles = folder.Name.StartsWith('.');
        IReadOnlyList<IStorageFile> files = await GetChildFiles(folder, extensions, showHiddenFiles);

        string? newBookmarkId = await folder.SaveBookmarkAsync();
        //Release the old bookmark if we got a new one
        if (newBookmarkId != null && bookmarkFolder != null)
        {
            await bookmarkFolder.ReleaseBookmarkAsync();
            bookmarkFolder.Dispose();
        }
        return (files, false, newBookmarkId);
    }

    private async Task<IReadOnlyList<IStorageFile>> GetChildFiles(IStorageFolder folder, List<string> extensions, bool showHiddenFiles)
    {
        try
        {
            IAsyncEnumerable<IStorageItem> items = folder.GetItemsAsync();

            List<IStorageFile> files = [];

            await foreach (IStorageItem item in items)
            {
                if (!showHiddenFiles && item.Name.StartsWith('.')) continue;
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
                    files.AddRange(await GetChildFiles(childFolder, extensions, showHiddenFiles));
                }
            }

            return files;
        }
        catch (Exception e)
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<IStorageFile>> OpenImageFiles()
    {
        TopLevel? target = getTarget();
        if (target == null) return [];
        IReadOnlyList<IStorageFile> files = await target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Folders",
            AllowMultiple = true,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });

        return files;
    }

    public async Task<string?> SaveImageFile(Bitmap bitmap, ImageFormat format, string suggestedName, string? bookmarkId)
    {
        TopLevel? target = getTarget();
        if (target == null) return null;

        //Get initial location from bookmark
        IStorageBookmarkFolder? bookmarkFolder = null;
        if (bookmarkId != null) bookmarkFolder = await target.StorageProvider.OpenFolderBookmarkAsync(bookmarkId);

        //Use pictures folder if there is no bookmark
        IStorageFolder? initialLocation = bookmarkFolder ?? await target.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Pictures);

        FilePickerFileType suggestedFileType =
            format == ImageFormat.Jpeg ? FilePickerFileTypes.ImageJpg : FilePickerFileTypes.ImagePng;

        //Open the file picker to choose a save location and name
        SaveFilePickerResult newFile = await target.StorageProvider.SaveFilePickerWithResultAsync(new FilePickerSaveOptions
        {
            Title = "Save Image",
            ShowOverwritePrompt = true,
            FileTypeChoices = [FilePickerFileTypes.ImageJpg, FilePickerFileTypes.ImagePng],
            SuggestedFileType = suggestedFileType,
            SuggestedStartLocation = initialLocation,
            SuggestedFileName = suggestedName,
        });

        if (newFile.File == null) return null;
        if (newFile.SelectedFileType != null) suggestedFileType = newFile.SelectedFileType;
        BitmapEncoderOptions options = suggestedFileType == FilePickerFileTypes.ImageJpg ? new JpegBitmapEncoderOptions() : new PngBitmapEncoderOptions();

        await using Stream stream = await newFile.File.OpenWriteAsync();
        bitmap.Save(stream, options);

        //Get a new bookmark for the new file's directory
        IStorageFolder? folder = await newFile.File.GetParentAsync();
        if (folder == null) return null;
        string? newBookmarkId = await folder.SaveBookmarkAsync();
        //Release the old bookmark if we got a new one
        if (newBookmarkId != null && bookmarkFolder != null)
        {
            await bookmarkFolder.ReleaseBookmarkAsync();
            bookmarkFolder.Dispose();
        }
        return newBookmarkId;
    }

    public async Task<T?> LoadObjectData<T>() where T : class?
    {
        try
        {
            string typeName = typeof(T).Name;
            string filePath = Path.Combine(_folderPath, $"{typeName}.txt");
            if (!File.Exists(filePath)) return null;
            await using FileStream fs = File.OpenRead(filePath);
            T? loadedData = JsonSerializer.Deserialize<T>(fs, _jsonOptions);
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
        await JsonSerializer.SerializeAsync(fs, data, _jsonOptions);
    }
}