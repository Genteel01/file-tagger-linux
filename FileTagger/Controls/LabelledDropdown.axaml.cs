using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using FileTagger.Extensions;

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

    public static readonly StyledProperty<string> OpenOnFocusTextProperty =
        AvaloniaProperty.Register<LabelledDropdown, string>(
            nameof(OpenOnFocusText),
            defaultValue: "",
            defaultBindingMode: BindingMode.OneWay);

    /// <summary>
    /// With this property set, the dropdown will only open on focus when <see cref="Text"/> is equal to this value
    /// </summary>
    public string OpenOnFocusText
    {
        get => GetValue(OpenOnFocusTextProperty);
        set => SetValue(OpenOnFocusTextProperty, value);
    }

    public event EventHandler? DropDownClosed;
    public event EventHandler<TextChangedEventArgs>? SearchFieldChanged;

    public LabelledDropdown()
    {
        InitializeComponent();
    }

    private void AutoCompleteBoxFocusGained(object? sender, FocusChangedEventArgs e)
    {
        if(sender is not AutoCompleteBox box) return;
        bool openedFromDropdownButton = e.NavigationMethod == NavigationMethod.Unspecified;
        if (!openedFromDropdownButton && !string.IsNullOrEmpty(OpenOnFocusText))
        {
            if(Text != OpenOnFocusText) return;
        }
        box.IsDropDownOpen = true;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (SearchFieldChanged != null)
        {
            List<TextBox> textBoxes = this.GetVisualDescendants<TextBox>().ToList();
            if (textBoxes.Count != 0)
            {
                textBoxes[0].TextChanged += SearchFieldChanged;
            }
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        if (SearchFieldChanged != null)
        {
            List<TextBox> textBoxes = this.GetVisualDescendants<TextBox>().ToList();
            if (textBoxes.Count != 0)
            {
                textBoxes[0].TextChanged -= SearchFieldChanged;
            }
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
}