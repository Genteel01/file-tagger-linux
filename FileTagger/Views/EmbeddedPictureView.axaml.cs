using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace FileTagger.Views;

public partial class EmbeddedPictureView : UserControl
{
    public EmbeddedPictureView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// <see cref="PicTypeSelector"/>'s Popup
    /// </summary>
    private Popup? _popup;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        PicTypeSelector.TemplateApplied += PicTypeSelectorTemplateApplied;
    }

    /// <summary>
    /// Set up the popup to allow pointer passthrough
    /// </summary>
    private void PicTypeSelectorTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        _popup = e.NameScope.Get<Popup>("PART_Popup");
        _popup.OverlayInputPassThroughElement = TopLevel.GetTopLevel(this);
        _popup.LostFocus += PicTypeSelectorDropDownLostFocus;
    }

    /// <summary>
    /// Set up the popup to refocus itself after clicking one of PicTypeSelector's buttons and to close the popup
    /// when it loses focus. Don't close the popup when the new focus is the ComboBox or a ComboBoxItem,
    /// because it closes itself in those cases
    /// </summary>
    private void PicTypeSelectorDropDownLostFocus(object? sender, FocusChangedEventArgs e)
    {
        if (e.NewFocusedElement is Button b && b.Parent == PicTypeSelector.Parent)
        {
            _popup?.Focus();
        }
        else if (e.NewFocusedElement != PicTypeSelector && e.NewFocusedElement is not ComboBoxItem)
        {
            _popup?.Close();
        }
    }
}