using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FileTagger.ViewModels;
using FileTagger.Views;
using FileTagger.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileTagger;

public class App : Application
{
    private IFileService? _fileService = null;
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
            desktop.MainWindow = new MainWindow();
            desktop.ShutdownRequested += DesktopOnShutdownRequested;

            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IFileService>(_ => new FileService(desktop.MainWindow));
            serviceCollection.AddSingleton<MainWindowViewModel>();
            serviceCollection.AddSingleton<EditPanelViewModel>();

            IServiceProvider services = serviceCollection.BuildServiceProvider();

            _fileService = services.GetRequiredService<IFileService>();
            await _fileService.LoadPreferenceData();

            MainWindowViewModel mainWindowViewModel = services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow.DataContext =  mainWindowViewModel;
            desktop.ShutdownRequested += DesktopOnShutdownRequested;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private bool _canClose = false;
    private async void DesktopOnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        e.Cancel = !_canClose;
        if (!_canClose && _fileService != null)
        {
            await _fileService.SavePreferenceData();
            _canClose = true;
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }
}