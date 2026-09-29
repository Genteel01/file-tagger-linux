using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileTagger.ViewModels;

public abstract partial class ViewModelBase : ObservableRecipient
{
    protected ViewModelBase()
    {
        ErrorMessages = [];
    }

    [ObservableProperty]
    public partial ObservableCollection<string> ErrorMessages { get; set; }
}