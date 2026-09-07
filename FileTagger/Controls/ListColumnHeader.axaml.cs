using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace FileTagger.Controls;

public partial class ListColumnHeader : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        TextBlock.TextProperty.AddOwner<ListColumnHeader>(new StyledPropertyMetadata<string?>(string.Empty, BindingMode.TwoWay, enableDataValidation: true));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public ListColumnHeader()
    {
        InitializeComponent();
    }
}