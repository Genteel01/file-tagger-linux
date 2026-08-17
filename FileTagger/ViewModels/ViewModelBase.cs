using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileTagger.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    protected ViewModelBase()
    {
        ErrorMessages = [];
    }

    [ObservableProperty]
    private ObservableCollection<string>? _errorMessages;
}