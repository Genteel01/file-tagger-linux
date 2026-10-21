using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FileTagger.Statics;

namespace FileTagger.ViewModels;

public partial class ColumnSelectorViewModel : ViewModelBase
{
    public class ListVisibilityData(string name, string label, bool isVisible)
    {
        public string Name { get; } = name;
        public string Label { get; set; } = label;
        public bool IsVisible { get; set; } = isVisible;
    }

    private readonly Window _dialog;

    public ObservableCollection<ListVisibilityData> ColumnsVisibility { get; } = [];

    public ColumnSelectorViewModel(Window dialog, Dictionary<string, bool> startingColumns)
    {
        _dialog = dialog;

        foreach (KeyValuePair<string, bool> column in startingColumns)
        {
            string label = TrackListData.PossibleListColumns[column.Key].Label;
            ColumnsVisibility.Add(new ListVisibilityData(column.Key, label, column.Value));
        }
    }

    [RelayCommand]
    private void Cancel() => _dialog.Close();

    [RelayCommand]
    private void ApplyChanges()
    {
        Dictionary<string, bool> columns =
            ColumnsVisibility.ToDictionary(column => column.Name, column => column.IsVisible);
        _dialog.Close(columns);
    }
}