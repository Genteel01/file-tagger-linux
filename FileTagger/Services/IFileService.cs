using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using FileTagger.Models;

namespace FileTagger.Services;

public interface IFileService
{
    public Task<(IReadOnlyList<IStorageFile>, bool)> OpenFilesRecursivelyAsync(List<string> extensions);
    public Task<IReadOnlyList<IStorageFile>> OpenImageFiles();
    public Preferences PreferenceData { get; protected set; }
    public void StorePreferenceItem(PropertyInfo property, object value);
    public Task<bool> LoadPreferenceData();
    public Task<bool> SavePreferenceData();
}