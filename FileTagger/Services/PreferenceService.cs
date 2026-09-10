using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using FileTagger.Models;

namespace FileTagger.Services;

public class PreferenceService(IFileService fileService) : IPreferenceService
{
    private Preferences _preferenceData = new Preferences();

    public Preferences GetPreferenceData()  => _preferenceData;

    public void StorePreferenceItem(PropertyInfo property, object value)
    {
        bool correctType = property.PropertyType == value.GetType();
        if(correctType)
        {
            property.SetValue(_preferenceData, value);
        }
        else
        {
            throw new InvalidCastException($"Property {property.Name} of Type {property.PropertyType} cannot be cast to type {value.GetType()}");
        }
    }

    public void StorePreferenceDictionaryValue<TK, TV>(PropertyInfo property, TK key, TV value) where TK : notnull
    {
        bool isDictionary = property.PropertyType.GetInterface(nameof(IDictionary)) != null;
        bool isRightTypes = property.PropertyType.GenericTypeArguments.Length == 2 &&
                            property.PropertyType.GenericTypeArguments[0] == typeof(TK) &&
                            property.PropertyType.GenericTypeArguments[1] == typeof(TV);
        if (isDictionary && isRightTypes)
        {
            IDictionary<TK, TV> dictionary = (IDictionary<TK, TV>)property.GetValue(_preferenceData)!;
            if (!dictionary.IsReadOnly)
            {
                dictionary[key] = value;
            }
            else
            {
                throw new ReadOnlyException($"Property {property.Name} of Type {property.PropertyType} is ReadOnly");
            }
        }
        else
        {
            throw new InvalidCastException($"Property {property.Name} of Type {property.PropertyType} is not a Dictionary<{typeof(TK).Name},{typeof(TV).Name}>");
        }
    }

    public async Task LoadPreferenceData()
    {
        Preferences? loadedData = await fileService.LoadObjectData<Preferences>();
        if (loadedData != null) _preferenceData = loadedData;
        _preferenceData.AddMissingColumnWidths();
    }

    public async Task SavePreferenceData()
    {
        await fileService.SaveJsonData(_preferenceData);
    }
}