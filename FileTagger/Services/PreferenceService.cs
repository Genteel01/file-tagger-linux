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
    public const int PreferenceErrorCode = 123456789;
    private Preferences _preferenceData = new Preferences();

    public UserPreferences UserPreferenceData { get; set; } = new UserPreferences();
    public SystemPreferences SystemPreferenceData { get; set; } = new SystemPreferences();

    public void StorePreferenceItem(PropertyInfo property, object value)
    {
        bool correctType = property.PropertyType == value.GetType();
        if(correctType)
        {
            if (property.DeclaringType == typeof(UserPreferences))
            {
                property.SetValue(UserPreferenceData, value);
            }
            else if (property.DeclaringType == typeof(SystemPreferences))
            {
                property.SetValue(SystemPreferenceData, value);
            }
            else
            {
                throw new InvalidCastException($"Property {property.Name} Declaring Type {property.DeclaringType} is not UserPreferences or SystemPreferences", PreferenceErrorCode);
            }
        }
        else
        {
            throw new InvalidCastException($"Property {property.Name} of Type {property.PropertyType} cannot be cast to type {value.GetType()}", PreferenceErrorCode);
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
            IDictionary<TK, TV> dictionary;
            if (property.DeclaringType == typeof(UserPreferences))
            {
                dictionary = (IDictionary<TK, TV>)property.GetValue(UserPreferenceData)!;
            }
            else if (property.DeclaringType == typeof(SystemPreferences))
            {
                dictionary = (IDictionary<TK, TV>)property.GetValue(SystemPreferenceData)!;
            }
            else
            {
                throw new InvalidCastException($"Property {property.Name} Declaring Type {property.DeclaringType} is not UserPreferences or SystemPreferences", PreferenceErrorCode);
            }
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
            throw new InvalidCastException($"Property {property.Name} of Type {property.PropertyType} is not a Dictionary<{typeof(TK).Name},{typeof(TV).Name}>", PreferenceErrorCode);
        }
    }

    public async Task LoadPreferenceData()
    {
        UserPreferences? loadedUserData = await fileService.LoadObjectData<UserPreferences>();
        if (loadedUserData != null) UserPreferenceData = loadedUserData;
        UserPreferenceData.AddMissingColumnWidths();
        SystemPreferences? loadedSystemData = await fileService.LoadObjectData<SystemPreferences>();
        if (loadedSystemData != null) SystemPreferenceData = loadedSystemData;
    }

    public async Task SavePreferenceData()
    {
        await fileService.SaveJsonData(UserPreferenceData);
        await fileService.SaveJsonData(SystemPreferenceData);
    }
}