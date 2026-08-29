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

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        //Allow us to save blank values in int? fields (year, track number, disc number)
        ATL.Settings.NullAbsentValues = true;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            desktop.ShutdownRequested += DesktopOnShutdownRequested;

            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IFileService>(_ => new FileService(desktop.MainWindow));
            serviceCollection.AddSingleton<MainWindowViewModel>();
            serviceCollection.AddSingleton<EditPanelViewModel>();

            IServiceProvider services = serviceCollection.BuildServiceProvider();

            MainWindowViewModel mainWindowViewModel = services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow.DataContext =  mainWindowViewModel;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task InitMainViewModelAsync()
    {
        //TODO load previously loaded files
    }

    private async void DesktopOnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        //TODO save which files are currently open, then close when done (uses Bookmarks probably?)
    }
}