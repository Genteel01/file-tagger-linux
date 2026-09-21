using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FileTagger.Models;
using FileTagger.Extensions;
using FileTagger.ViewModels;
using FileTagger.Views;
using FileTagger.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileTagger;

public class App : Application
{
    private IPreferenceService? _preferenceService = null;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        #if DEBUG
        this.AttachDeveloperTools();
        #endif
        //Allow us to save blank values in int? fields (year, track number, disc number)
        ATL.Settings.NullAbsentValues = true;
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IFileService, FileService>();
            serviceCollection.AddSingleton<IPreferenceService, PreferenceService>();
            serviceCollection.AddSingleton<IImageService, ImageService>();
            serviceCollection.AddSingleton<MainWindowViewModel>();
            serviceCollection.AddSingleton<EditPanelViewModel>();
            serviceCollection.AddSingleton<Func<TopLevel?>>(_ => () => TopLevel.GetTopLevel(desktop.MainWindow));

            IServiceProvider services = serviceCollection.BuildServiceProvider();

            _preferenceService = services.GetRequiredService<IPreferenceService>();
            await _preferenceService.LoadPreferenceData();

            SystemPreferences preferences = _preferenceService.SystemPreferenceData;

            WindowState startingWindowState = preferences.IsMaximised ? WindowState.Maximized : WindowState.Normal;
            MainWindowViewModel mainWindowViewModel = services.GetRequiredService<MainWindowViewModel>();

            MainWindow mainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel,
                WindowState = startingWindowState,
                Width = preferences.WindowSize.Width,
                Height = preferences.WindowSize.Height
            };
            //Fix an error when the window width and height are 0
            if (mainWindow.Width == 0) mainWindow.Width = mainWindow.ClientSize.Width;
            if (mainWindow.Height == 0) mainWindow.Height = mainWindow.ClientSize.Height;
            //Add a callback so we can store the size of the window when it changes
            mainWindow.Resized += (sender, _) => { if (sender is Window window) window.StoreWindowState(_preferenceService); };

            mainWindow.TemplateApplied += (_, _) => { mainWindow.Hide(); };
            desktop.MainWindow = mainWindow;
            await mainWindowViewModel.OpenInitialFiles();
            desktop.MainWindow.Show();
            desktop.ShutdownRequested += DesktopOnShutdownRequested;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private bool _canClose = false;
    private async void DesktopOnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        e.Cancel = !_canClose;
        if (!_canClose && _preferenceService != null)
        {
            await _preferenceService.SavePreferenceData();
            _canClose = true;
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }
}
