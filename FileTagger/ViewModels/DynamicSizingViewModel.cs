using System;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileTagger.Models;
using FileTagger.Services;
using FileTagger.Statics;

namespace FileTagger.ViewModels;

public abstract partial class DynamicSizingViewModel(Func<TopLevel?> getTopLevel, IPreferenceService preferenceService) : ViewModelBase
{
    [ObservableProperty]
    public partial bool IsCompact { get; set; }

    [ObservableProperty]
    public partial MyThemes.LayoutSize SelectedLayoutSize { get; set; }

    partial void OnSelectedLayoutSizeChanged(MyThemes.LayoutSize value)
    {
        CheckScreenHeight();
        PropertyInfo compactLayoutProperty = typeof(UserPreferences).GetProperty(nameof(UserPreferences.RequestedLayoutSize))!;
        preferenceService.StorePreferenceItem(compactLayoutProperty, value);
    }

    [ObservableProperty]
    public partial MyThemes.LayoutSize[] Layouts { get; set; } = Enum.GetValues<MyThemes.LayoutSize>();


    private const int HeightThreshold = 1080;

    public void CheckScreenHeight()
    {
        switch (SelectedLayoutSize)
        {
            case MyThemes.LayoutSize.Compact:
                IsCompact = true;
                return;
            case MyThemes.LayoutSize.Standard:
                IsCompact = false;
                return;
            case MyThemes.LayoutSize.Automatic:
            default:
            {
                TopLevel? topLevel = getTopLevel();
                if (topLevel?.Screens == null) return;
                Screen? activeScreen = topLevel.Screens.ScreenFromTopLevel(topLevel);
                if (activeScreen == null) return;
                int height = activeScreen.Bounds.Height;
                IsCompact = height <= HeightThreshold;
                break;
            }
        }
    }

    [RelayCommand]
    private void ChangeLayout(MyThemes.LayoutSize value) => SelectedLayoutSize = value;
}