using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
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

    private AutoCompleteBox? _searchField = null;
    private TextBox? _textBox = null;
    private Button? _expandButton = null;
    private TopLevel? _topLevel = null;

    public LabelledDropdown()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (_searchField == null)
        {
            List<AutoCompleteBox> boxes = this.GetVisualDescendants<AutoCompleteBox>().ToList();
            if (boxes.Count != 0)
            {
                _searchField = boxes[0];
            }
        }

        if (_textBox == null)
        {
            List<TextBox> textBoxes = this.GetVisualDescendants<TextBox>().ToList();
            if (textBoxes.Count != 0)
            {
                _textBox = textBoxes[0];
            }
        }

        if (_expandButton == null)
        {
            List<Button> buttons = this.GetVisualDescendants<Button>().ToList();
            if (buttons.Count != 0)
            {
                _expandButton = buttons[0];
            }
        }

        _topLevel = TopLevel.GetTopLevel(this);

        _expandButton?.GotFocus += ExpandButtonFocused;
        _searchField?.DropDownOpening += SearchField_OnDropDownOpening;
        _textBox?.LosingFocus += TextBoxLosingFocus;
        _textBox?.TextChanged += SearchFieldChanged;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _textBox?.LosingFocus -= TextBoxLosingFocus;
        _textBox?.TextChanged -= SearchFieldChanged;
        _expandButton?.GotFocus -= ExpandButtonFocused;
        _searchField?.DropDownOpening -= SearchField_OnDropDownOpening;
    }

    /// <summary>
    /// When we focus the search field, open the dropdown if the Text matches <see cref="OpenOnFocusText"/>,
    /// or OpenOnFocusText is blank. Skip if the focus is travelling from the TextBox to the TextBox due to <see cref="TextBoxLosingFocus"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AutoCompleteBoxFocusGained(object? sender, FocusChangedEventArgs e)
    {
        if(sender is not AutoCompleteBox box) return;
        if(e.NewFocusedElement == e.OldFocusedElement) return;
        if (!string.IsNullOrEmpty(OpenOnFocusText))
        {
            if(Text != OpenOnFocusText) return;
        }
        box.IsDropDownOpen = true;
    }

    /// <summary>
    /// When we click the Expand button, focus the textbox and open the dropdown
    /// </summary>
    private void ExpandButtonFocused(object? sender, FocusChangedEventArgs e)
    {
        _textBox?.Focus();
        _searchField?.IsDropDownOpen = true;
    }

    private void SearchField_OnDropDownClosed(object? sender, EventArgs e)
    {
        DropDownClosed?.Invoke(sender, e);
    }

    /// <summary>
    /// When the dropdown opens, get it and set it so it doesn't block input
    /// </summary>
    private void SearchField_OnDropDownOpening(object? sender, CancelEventArgs e)
    {
        if (sender is not AutoCompleteBox box) return;

        List<Popup> popups = box.GetVisualDescendants<Popup>().ToList();
        if (popups.Count != 0)
        {
            Popup popup = popups[0];
            popup.OverlayInputPassThroughElement = _topLevel;
        }
    }

    /// <summary>
    /// When we are losing focus on the search field, if we clicked the Expand button and the dropdown is open,
    /// re-focus the TextBox, and the dropdown will automatically close
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBoxLosingFocus(object? sender, FocusChangingEventArgs e)
    {
        if (_searchField?.IsDropDownOpen == true && e.NewFocusedElement == _expandButton)
        {
            e.TrySetNewFocusedElement(_textBox);
        }
    }
}