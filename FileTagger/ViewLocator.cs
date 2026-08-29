using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FileTagger.ViewModels;
using FileTagger.Views;

namespace FileTagger;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        return data switch
        {
            MainWindowViewModel => new MainWindow(),
            EditPanelViewModel => new EditPanel(),
            _ => new TextBlock { Text = $"No view for {data?.GetType().Name}" }
        };
    }


    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}