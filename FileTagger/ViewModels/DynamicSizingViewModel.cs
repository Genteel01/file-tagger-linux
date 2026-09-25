using System;
using Avalonia.Controls;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileTagger.ViewModels;

public abstract partial class DynamicSizingViewModel(Func<TopLevel?> getTopLevel) : ViewModelBase
{
    [ObservableProperty]
    public partial bool IsShort { get; set; }

    private const int HeightThreshold = 850;

    public void CheckScreenHeight()
    {
        TopLevel? topLevel = getTopLevel();
        if (topLevel?.Screens == null) return;
        Screen? activeScreen = topLevel.Screens.ScreenFromTopLevel(topLevel);
        if (activeScreen == null) return;
        int height = activeScreen.Bounds.Height;
        if (height < HeightThreshold) IsShort = true;
    }
}