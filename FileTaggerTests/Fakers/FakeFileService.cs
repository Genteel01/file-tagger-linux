using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Commons;
using FileTagger.Services;

namespace FileTaggerTests.Fakers;

public class FakeFileService : IFileService
{
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { IncludeFields = true, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals };
    private Dictionary<string, JsonDocument> _documents = new Dictionary<string, JsonDocument>();
    public async Task<IReadOnlyList<IStorageFile>> OpenBookmarkedFilesAsync(List<string> extensions, string bookmarkId)
    {
        return [];
    }

    public async Task<(IReadOnlyList<IStorageFile> files, bool cancelled, string? bookmarkId)> OpenFilesRecursivelyAsync(List<string> extensions, string? bookmarkId = null)
    {
        return ([], false, null);
    }

    public async Task<IReadOnlyList<IStorageFile>> OpenImageFiles()
    {
        return [];
    }

    public async Task<string?> SaveImageFile(Bitmap bitmap, ImageFormat format, string suggestedName, string? bookmarkId)
    {
        return null;
    }

    public async Task<T?> LoadObjectData<T>() where T : class?
    {
        try
        {
            string typeName = typeof(T).Name;
            if (!_documents.ContainsKey(typeName)) return null;

            T? loadedData = _documents[typeName].Deserialize<T>(_jsonOptions);
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
        _documents[typeName] = JsonSerializer.SerializeToDocument(data, _jsonOptions);
    }
}