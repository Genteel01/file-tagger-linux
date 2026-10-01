using Avalonia.Controls;
#if DEBUG
using Avalonia.Rendering;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using FileTagger.Statics;
using FileTagger.ViewModels;
#endif

namespace FileTagger.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        #if DEBUG
        //RendererDiagnostics.DebugOverlays = RendererDebugOverlays.RenderTimeGraph;
        #endif
        InitializeComponent();
    }

    #if DEBUG
    /// <summary>
    /// In debug mode use the tilde key to switch themes for easier testing
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        Root.KeyDown += (sender, args) =>
        {
            if(args.Key is not Key.OemTilde) return;
            if(sender is not Grid grid) return;
            if(grid.DataContext is not MainWindowViewModel vm) return;

            ThemeVariant newTheme = MyThemes.GetOppositeTheme(vm.SelectedTheme);
            vm.ChangeSelectedTheme(newTheme);
        };
    }
    #endif
}