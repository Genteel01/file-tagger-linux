using System;
using System.Collections.Generic;
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
        bool isDictionary = property.PropertyType.GetInterface(typeof(IDictionary<TK, TV>).Name) != null;
        if (isDictionary)
        {
            IDictionary<TK, TV> dictionary = (IDictionary<TK, TV>)property.GetValue(_preferenceData)!;
            dictionary[key] = value;
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