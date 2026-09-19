using System;
using System.Linq;
using Avalonia.Controls;

namespace FileTagger.SharedEventHandlers;

public static class TextValidation
{
    public static readonly EventHandler<TextChangedEventArgs> OnlyAllowNumberInput = (sender, args) =>
    {
        if (sender is not TextBox box) return;
        string text = box.Text ?? "";
        string newString = string.Concat(text.Where(char.IsDigit));
        int sizeDifference = text.Length - newString.Length;

        if (sizeDifference > 0)
        {
            box.CaretIndex -= sizeDifference;
            box.Text = newString;
        }
    };
}