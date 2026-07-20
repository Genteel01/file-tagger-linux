using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using FileTagger.ViewModels;
using FileTagger.Views;
using FileTagger.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileTagger;

public partial class App : Application
{
    // This is a reference to our MainViewModel which we use to save the list on shutdown. You can also use Dependency Injection
    // in your App.
    private readonly MainWindowViewModel _mainViewModel = new MainWindowViewModel();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };
            desktop.ShutdownRequested += DesktopOnShutdownRequested;

            var services = new ServiceCollection();

            services.AddSingleton<IFileService>(x => new FileService(desktop.MainWindow));

            Services = services.BuildServiceProvider();
        }

        base.OnFrameworkInitializationCompleted();

        // Init the MainViewModel
        await InitMainViewModelAsync();
    }

    // Optional: Load data from disc
    private async Task InitMainViewModelAsync()
    {
        //TODO load previously loaded files
    }

    // We want to save our ToDoList before we actually shutdown the App. As File I/O is async, we need to wait until file is closed
    // before we can actually close this window

    private bool _canClose; // This flag is used to check if window is allowed to close
    private async void DesktopOnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        //TODO which files are currently open
    }

    public new static App? Current => Application.Current as App;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider? Services { get; private set; }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}