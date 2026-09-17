using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace FileTagger.Controls;

public partial class LabelledDropdown : UserControl
{
    public static readonly DirectProperty<LabelledDropdown, string> LabelTextProperty =
        AvaloniaProperty.RegisterDirect<LabelledDropdown, string>(
            nameof(LabelText),
            o => o.LabelText,
            unsetValue: "",
            defaultBindingMode: BindingMode.OneWay);

    public string LabelText
    {
        get;
        set => SetAndRaise(LabelTextProperty, ref field, value);
    }

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AutoCompleteBox.ItemsSourceProperty.AddOwner<LabelledDropdown>();

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly StyledProperty<string?> TextProperty =
        TextBlock.TextProperty.AddOwner<LabelledDropdown>(new StyledPropertyMetadata<string?>(string.Empty, BindingMode.TwoWay, enableDataValidation: true));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<AutoCompleteFilterPredicate<string?>?> TextFilterProperty =
        AutoCompleteBox.TextFilterProperty.AddOwner<LabelledDropdown>();

    public AutoCompleteFilterPredicate<string?>? TextFilter
    {
        get => GetValue(TextFilterProperty);
        set => SetValue(TextFilterProperty, value);
    }

    public event EventHandler? DropDownClosed;
    public event EventHandler? SearchFieldChanged;

    public LabelledDropdown()
    {
        InitializeComponent();
    }

    private void AutoCompleteBoxFocusGained(object? sender, FocusChangedEventArgs e)
    {
        if (sender is AutoCompleteBox box)
        {
            box.IsDropDownOpen = true;
        }
    }

    private void ExpandButtonTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Button b) return;

        IEnumerable<ILogical> siblings = b.GetLogicalSiblings();
        foreach (ILogical sibling in siblings)
        {
            if (sibling is AutoCompleteBox box)
            {
                box.Focus();
            }
        }
    }

    private void SearchField_OnDropDownClosed(object? sender, EventArgs e)
    {
        DropDownClosed?.Invoke(sender, e);
    }

    private void SearchField_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        SearchFieldChanged?.Invoke(sender, e);
    }
}