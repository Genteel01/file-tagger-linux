using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
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
        //Allow us to save blank values in int? fields (year, track number, disc number)
        ATL.Settings.NullAbsentValues = true;
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };
            desktop.ShutdownRequested += DesktopOnShutdownRequested;

            ServiceCollection services = new ServiceCollection();

            services.AddSingleton<IFileService>(_ => new FileService(desktop.MainWindow));

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
}