using System;
using System.Reflection;
using System.Threading.Tasks;
using FileTagger.Models;

namespace FileTagger.Services;

public interface IPreferenceService
{
    public UserPreferences UserPreferenceData { get; protected set; }
    public SystemPreferences SystemPreferenceData { get; protected set; }
    public event EventHandler? UserPreferencesSet;
    public void StorePreferenceItem(PropertyInfo property, object value);
    public void StorePreferenceDictionaryValue<TK, TV>(PropertyInfo property, TK key, TV value) where TK : notnull;
    public Task LoadPreferenceData();
    public Task SavePreferenceData();
    public void ResetUserPreferences();
}