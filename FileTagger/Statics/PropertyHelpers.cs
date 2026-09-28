using System.Linq;
using System.Reflection;
using FileTagger.Extensions;
using FileTagger.ViewModels;

namespace FileTagger.Statics;

public static class PropertyHelpers
{
    /// <summary>
    /// Gets a property of T in a way that is forgiving of non-exact strings
    /// </summary>
    public static PropertyInfo? GetProperty<T>(string searchTerm)
    {
        PropertyInfo? property = typeof(T).GetProperty(searchTerm);
        if (property != null) return property;

        string wordsCapitalised = CapitaliseWords(searchTerm);
        property = typeof(T).GetProperty(wordsCapitalised);
        if (property != null) return property;

        string noSpaces = searchTerm.Replace(" ", "");
        property = typeof(T).GetProperty(noSpaces);
        if (property != null) return property;

        string wordsCapitalisedNoSpaces = wordsCapitalised.Replace(" ", "");
        property = typeof(T).GetProperty(wordsCapitalisedNoSpaces);
        if (property != null) return property;

        //Special cases for TrackViewModel
        if (typeof(T) == typeof(TrackViewModel))
        {
            if (searchTerm == "Disc" || wordsCapitalised == "Disc") property = typeof(T).GetProperty(nameof(TrackViewModel.DiscNumber));
            if (property != null) return property;
            if (searchTerm == "Track" || wordsCapitalised == "Track") property = typeof(T).GetProperty(nameof(TrackViewModel.TrackNumber));
        }
        return property;
    }

    /// <summary>
    /// Capitalises the first letter and all letters following whitespace
    /// </summary>
    private static string CapitaliseWords(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        char[] chars = input.ToCharArray();
        int[] whitespaceIndices = chars.FindIndices(char.IsWhiteSpace).Prepend(-1).ToArray();

        foreach (int i in whitespaceIndices)
        {
            if (i + 1 < chars.Length)
            {
                chars[i + 1] = char.ToUpperInvariant(input[i + 1]);
            }
        }

        return new string(chars);
    }
}