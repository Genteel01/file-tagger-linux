using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FileTagger.ViewModels;

namespace FileTagger.Statics;

public static class PropertyHelpers
{
    /// <summary>
    /// Gets a property of T using a name that may not match case and may have spaces.
    /// Includes special cases for TrackViewModel to return "Disc" as DiscNumber and "Track" as TrackNumber.
    /// </summary>
    public static PropertyInfo? GetProperty<T>(string searchTerm)
    {
        List<PropertyInfo> properties = typeof(T).GetProperties().ToList();

        PropertyInfo? property = properties.Find(p => string.Equals(p.Name, searchTerm, StringComparison.OrdinalIgnoreCase));
        if (property != null) return property;

        string noSpaces = searchTerm.Replace(" ", "");
        property = properties.Find(p => string.Equals(p.Name, noSpaces, StringComparison.OrdinalIgnoreCase));
        if (property != null) return property;

        //Special cases for TrackViewModel
        if (typeof(T) == typeof(TrackViewModel))
        {
            if (string.Equals(searchTerm, "Disc", StringComparison.OrdinalIgnoreCase)) property = typeof(T).GetProperty(nameof(TrackViewModel.DiscNumber));
            if (property != null) return property;
            if (string.Equals(searchTerm, "Track", StringComparison.OrdinalIgnoreCase)) property = typeof(T).GetProperty(nameof(TrackViewModel.TrackNumber));
        }
        return property;
    }
}