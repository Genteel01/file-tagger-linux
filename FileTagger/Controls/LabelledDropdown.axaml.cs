using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace FileTagger.Controls;

[TemplatePart("PART_Button", typeof(Button), IsRequired = true)]
[TemplatePart("PART_AutoCompleteBox", typeof(AutoCompleteBox), IsRequired = true)]
public class LabelledDropdown : TemplatedControl
{
    public static readonly StyledProperty<string> LabelTextProperty =
        AvaloniaProperty.Register<LabelledDropdown, string>(
            nameof(LabelText),
            defaultValue: "",
            defaultBindingMode: BindingMode.OneWay);

    public string LabelText
    {
        get => GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
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

    public static readonly StyledProperty<IBrush?> ButtonBackgroundProperty =
        Border.BackgroundProperty.AddOwner<LabelledDropdown>();

    public IBrush? ButtonBackground
    {
        get => GetValue(ButtonBackgroundProperty);
        set => SetValue(ButtonBackgroundProperty, value);
    }

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

    private AutoCompleteBox? _autoCompleteBox;
    private TextBox? _textBox;
    private TextPresenter? _textPresenter;
    private Popup? _popup;
    private Button? _button;
    private TopLevel? _topLevel;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _button?.GotFocus -= ExpandButtonFocused;
        _autoCompleteBox?.TemplateApplied -= OnAutoCompleteBoxApplyTemplate;
        _autoCompleteBox?.GotFocus -= AutoCompleteBoxFocusGained;
        _autoCompleteBox?.DropDownClosed -= DropDownClosed;

        _topLevel = TopLevel.GetTopLevel(this);
        _button = e.NameScope.Get<Button>("PART_Button");
        _autoCompleteBox = e.NameScope.Get<AutoCompleteBox>("PART_AutoCompleteBox");

        _autoCompleteBox.TemplateApplied += OnAutoCompleteBoxApplyTemplate;
        _autoCompleteBox.GotFocus += AutoCompleteBoxFocusGained;
        _autoCompleteBox.DropDownClosed += DropDownClosed;
        _button.GotFocus += ExpandButtonFocused;
    }

    private void OnAutoCompleteBoxApplyTemplate(object?  sender, TemplateAppliedEventArgs e)
    {
        _textBox?.TemplateApplied -= OnTextBoxApplyTemplate;
        _textBox?.LosingFocus -= TextBoxLosingFocus;
        _textBox?.TextChanged -= SearchFieldChanged;

        _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
        _popup = e.NameScope.Find<Popup>("PART_Popup");

        _textBox?.TemplateApplied += OnTextBoxApplyTemplate;
        _textBox?.LosingFocus += TextBoxLosingFocus;
        _textBox?.TextChanged += SearchFieldChanged;
        _popup?.OverlayInputPassThroughElement = _topLevel;
    }

    private void OnTextBoxApplyTemplate(object?  sender, TemplateAppliedEventArgs e)
    {
        _textPresenter = e.NameScope.Get<TextPresenter>("PART_TextPresenter");
    }

    /// <summary>
    /// Automatically set the line height of the text box to fill all space available
    /// </summary>
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (!e.HeightChanged) return;
        if (_textPresenter is { Bounds.Height: > 0 })
        {
            _textBox?.LineHeight = _textPresenter.Bounds.Height;
        }
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
        _autoCompleteBox?.IsDropDownOpen = true;
    }

    /// <summary>
    /// When we are losing focus on the search field, if we clicked the Expand button and the dropdown is open,
    /// re-focus the TextBox, and the dropdown will automatically close
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBoxLosingFocus(object? sender, FocusChangingEventArgs e)
    {
        if (_autoCompleteBox?.IsDropDownOpen == true && e.NewFocusedElement == _button)
        {
            e.TrySetNewFocusedElement(_textBox);
        }
    }
}