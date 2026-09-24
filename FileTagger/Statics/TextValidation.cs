using System;
using System.Linq;
using Avalonia.Controls;
using FileTagger.Statics;

namespace FileTagger.Statics;

[Flags]
public enum NumberValidationFlags
{
    AllowLeadingWhitespace = 1,
    AllowMidWhitespace = 2,
    AllowTrailingWhitespace = 4,
    AllowUnchangedField = 16
}

public static class TextValidation
{
    public static readonly EventHandler<TextChangedEventArgs> OnlyAllowNumberInput = (sender, _) =>
    {
        if (sender is not TextBox { Text: not null } box) return;
        string text = box.Text;
        bool allowLeadingWhitespace = false;
        bool allowMidWhitespace = false;
        bool allowTrailingWhitespace = false;
        bool allowUnchanged = false;
        if (box.Tag is NumberValidationFlags flags)
        {
            allowLeadingWhitespace = flags.HasFlag(NumberValidationFlags.AllowLeadingWhitespace);
            allowMidWhitespace = flags.HasFlag(NumberValidationFlags.AllowMidWhitespace);
            allowTrailingWhitespace = flags.HasFlag(NumberValidationFlags.AllowTrailingWhitespace);
            allowUnchanged = flags.HasFlag(NumberValidationFlags.AllowUnchangedField);
        }

        if (allowUnchanged && text.Trim() == Consts.UnchangedField) return;

        string newString = string.Concat(text.Where(c => char.IsDigit(c) || char.IsWhiteSpace(c)));
        bool entirelyWhitespace = string.IsNullOrWhiteSpace(newString);
        if (!allowTrailingWhitespace && !entirelyWhitespace) newString = newString.TrimEnd();
        if (!allowLeadingWhitespace && !entirelyWhitespace) newString = newString.TrimStart();
        if (!allowMidWhitespace && !entirelyWhitespace)
        {
            int leadingWhitespaceCount = newString.Length - newString.TrimStart().Length;
            int trailingWhitespaceCount = newString.Length - newString.TrimEnd().Length;
            string leadingWhitespace = newString.Substring(0, leadingWhitespaceCount);
            string trailingWhitespace = newString.Substring(newString.Length - trailingWhitespaceCount);
            string middle = newString.Substring(leadingWhitespaceCount, newString.Length - leadingWhitespaceCount - trailingWhitespaceCount);
            middle = string.Concat(middle.Where(char.IsDigit));
            newString = leadingWhitespace + middle + trailingWhitespace;
        }

        int sizeDifference = text.Length - newString.Length;

        if (sizeDifference > 0)
        {
            box.CaretIndex -= sizeDifference;
            box.Text = newString;
        }
    };
}