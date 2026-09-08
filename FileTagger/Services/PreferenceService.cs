using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FileTagger.Models;

namespace FileTagger.Services;

public class PreferenceService(IFileService fileService) : IPreferenceService
{
    public Preferences PreferenceData { get; set; } = new Preferences();

    public void StorePreferenceItem(PropertyInfo property, object value)
    {
        bool correctType = property.PropertyType == value.GetType();
        if(correctType) property.SetValue(PreferenceData, value);
    }

    public void StorePreferenceDictionaryValue<TK, TV>(PropertyInfo property, TK key, TV value) where TK : notnull
    {
        bool isDictionary = property.PropertyType.GetInterface(typeof(IDictionary<TK, TV>).Name) != null;
        if (isDictionary)
        {
            IDictionary<TK, TV> dictionary = (IDictionary<TK, TV>)property.GetValue(PreferenceData)!;
            dictionary[key] = value;
        }
    }

    public async Task LoadPreferenceData()
    {
        PreferenceData = await fileService.LoadObjectData<Preferences>() ??  new Preferences();
        PreferenceData.AddMissingColumnWidths();
    }

    public async Task SavePreferenceData()
    {
        await fileService.SaveJsonData(PreferenceData);
    }
}