using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Controls.Primitives;

namespace FileTagger.Controls;

public class ListColumnHeader : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty =
        TextBlock.TextProperty.AddOwner<ListColumnHeader>(new StyledPropertyMetadata<string?>(string.Empty, BindingMode.TwoWay, enableDataValidation: true));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<bool> ShowDownArrowProperty =
        AvaloniaProperty.Register<ListColumnHeader, bool>(
            nameof(ShowDownArrow),
            defaultBindingMode: BindingMode.OneWay);

    public bool ShowDownArrow
    {
        get => GetValue(ShowDownArrowProperty);
        set => SetValue(ShowDownArrowProperty, value);
    }

    public static readonly StyledProperty<bool> ShowUpArrowProperty =
        AvaloniaProperty.Register<ListColumnHeader, bool>(
            nameof(ShowUpArrow),
            defaultBindingMode: BindingMode.OneWay);

    public bool ShowUpArrow
    {
        get => GetValue(ShowUpArrowProperty);
        set => SetValue(ShowUpArrowProperty, value);
    }

    public static readonly StyledProperty<bool> ShowArrowsProperty =
        AvaloniaProperty.Register<ListColumnHeader, bool>(
            nameof(ShowArrows),
            defaultBindingMode: BindingMode.OneWay);

    public bool ShowArrows
    {
        get => GetValue(ShowArrowsProperty);
        set => SetValue(ShowArrowsProperty, value);
    }
}