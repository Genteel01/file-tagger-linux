using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace FileTagger.Controls;

public class DialogControls : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="ConfirmCommand" /> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> ConfirmCommandProperty = AvaloniaProperty.Register<DialogControls, ICommand?>(nameof (ConfirmCommand), enableDataValidation: true);

    /// <summary>
    /// Defines the <see cref="CancelCommand" /> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> CancelCommandProperty = AvaloniaProperty.Register<DialogControls, ICommand?>(nameof (CancelCommand), enableDataValidation: true);

    /// <summary>
    /// Defines the <see cref="ConfirmCommandParameter" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ConfirmCommandParameterProperty = AvaloniaProperty.Register<DialogControls, object?>(nameof (ConfirmCommandParameter));

    /// <summary>
    /// Defines the <see cref="ConfirmCommandParameter" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CancelCommandParameterProperty = AvaloniaProperty.Register<DialogControls, object?>(nameof (CancelCommandParameter));

    /// <summary>
    /// Defines the <see cref="ConfirmContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ConfirmContentProperty = AvaloniaProperty.Register<DialogControls, object?>(nameof (ConfirmContent));

    /// <summary>
    /// Defines the <see cref="CancelContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CancelContentProperty = AvaloniaProperty.Register<DialogControls, object?>(nameof (CancelContent));

    /// <summary>
    /// Gets or sets an <see cref="T:System.Windows.Input.ICommand" /> to be invoked when the Confirm button is clicked.
    /// </summary>
    public ICommand? ConfirmCommand
    {
        get => GetValue(ConfirmCommandProperty);
        set => SetValue(ConfirmCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets an <see cref="T:System.Windows.Input.ICommand" /> to be invoked when the Cancel button is clicked.
    /// </summary>
    public ICommand? CancelCommand
    {
        get => GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a parameter to be passed to the <see cref="CancelCommand" />.
    /// </summary>
    public object? CancelCommandParameter
    {
        get => GetValue(CancelCommandParameterProperty);
        set => SetValue(CancelCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets a parameter to be passed to the <see cref="ConfirmCommand" />.
    /// </summary>
    public object? ConfirmCommandParameter
    {
        get => GetValue(ConfirmCommandParameterProperty);
        set => SetValue(ConfirmCommandParameterProperty, value);
    }

    /// <summary>
    /// The Content of the Confirm button
    /// </summary>
    public object? ConfirmContent
    {
        get => GetValue(ConfirmContentProperty);
        set => SetValue(ConfirmContentProperty, value);
    }

    /// <summary>
    /// The Content of the Cancel button
    /// </summary>
    public object? CancelContent
    {
        get => GetValue(CancelContentProperty);
        set => SetValue(CancelContentProperty, value);
    }
}