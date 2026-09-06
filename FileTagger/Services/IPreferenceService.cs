using System.Reflection;
using System.Threading.Tasks;
using FileTagger.Models;

namespace FileTagger.Services;

public interface IPreferenceService
{
    public Preferences PreferenceData { get; protected set; }
    public void StorePreferenceItem(PropertyInfo property, object value);
    public Task LoadPreferenceData();
    public Task SavePreferenceData();
}