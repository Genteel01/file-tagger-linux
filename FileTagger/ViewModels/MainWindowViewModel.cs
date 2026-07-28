using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileTagger.Models;
using FileTagger.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileTagger.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _fileText;

    /// <summary>
    /// Gets a collection of <see cref="ToDoItem"/> which allows adding and removing items
    /// </summary>
    public ObservableCollection<ToDoItemViewModel> ToDoItems { get; } = new ObservableCollection<ToDoItemViewModel>();

    /// <summary>
    /// Gets or set the content for new Items to add. If this string is not empty, the AddItemCommand will be enabled automatically
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddItemCommand))]
    // This attribute will invalidate the command each time this property changes
    public string? newItemContent;

    /// <summary>
    /// Returns if a new Item can be added. We require to have the NewItem some Text
    /// </summary>
    private bool CanAddItem() => !string.IsNullOrWhiteSpace(NewItemContent);

    /// <summary>
    /// This command is used to add a new Item to the List
    /// </summary>
    [RelayCommand (CanExecute = nameof(CanAddItem))]
    private void AddItem()
    {
        // Add a new item to the list
        ToDoItems.Add(new ToDoItemViewModel() {Content = NewItemContent});

        // reset the NewItemContent
        NewItemContent = null;
    }

    /// <summary>
    /// Removes the given Item from the list
    /// </summary>
    /// <param name="item">the item to remove</param>
    [RelayCommand]
    private void RemoveItem(ToDoItemViewModel item)
    {
        // Remove the given item from the list
        ToDoItems.Remove(item);
    }

    [RelayCommand]
    private async Task OpenFile(CancellationToken token)
    {
        ErrorMessages?.Clear();
        try
        {
            var filesService = App.Current?.Services?.GetService<IFileService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            var file = await filesService.OpenFileAsync();
            if (file is null) return;

            // Limit the text file to 1MB so that the demo wont lag.
            if ((await file.GetBasicPropertiesAsync()).Size <= 1024 * 1024 * 1)
            {
                await using var readStream = await file.OpenReadAsync();
                using var reader = new StreamReader(readStream);
                FileText = await reader.ReadToEndAsync(token);
            }
            else
            {
                throw new Exception("File exceeded 1MB limit.");
            }
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }
    }

    [RelayCommand]
    private async Task OpenFolders(CancellationToken token)
    {
        ErrorMessages?.Clear();
        try
        {
            var filesService = App.Current?.Services?.GetService<IFileService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            var folders = await filesService.OpenFoldersAsync();

            FileText = "";
            foreach (IStorageFolder storageFolder in folders)
            {
                FileText += storageFolder.Name + " at " + storageFolder.Path + "\n";
            }
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }
    }

    [RelayCommand]
    private async Task OpenMusicFiles(CancellationToken token)
    {
        ErrorMessages?.Clear();
        try
        {
            var filesService = App.Current?.Services?.GetService<IFileService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            var files = await filesService.OpenFilesRecursivelyAsync();

            FileText = "";
            foreach (IStorageFile file in files)
            {
                if (file.Name.EndsWith(".mp3") ||  file.Name.EndsWith(".wav") || file.Name.EndsWith(".flac"))
                {
                    FileText += file.Name + " || " + file.Path + "\n";
                }
            }
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }
    }

    [RelayCommand]
    private async Task SaveFile()
    {
        ErrorMessages?.Clear();
        try
        {
            var filesService = App.Current?.Services?.GetService<IFileService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            var file = await filesService.SaveFileAsync();
            if (file is null) return;


            // Limit the text file to 1MB so that the demo wont lag.
            if (FileText?.Length <= 1024 * 1024 * 1)
            {
                var stream = new MemoryStream(Encoding.Default.GetBytes((string)FileText));
                await using var writeStream = await file.OpenWriteAsync();
                await stream.CopyToAsync(writeStream);
            }
            else
            {
                throw new Exception("File exceeded 1MB limit.");
            }
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }
    }
}