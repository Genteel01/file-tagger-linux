using System.Reflection;
using FileTagger.Models;
using FileTagger.Services;
using FileTaggerTests.Fakers;

namespace FileTaggerTests.Services;

public class PreferenceServiceTests
{
    private static PreferenceService CreateMockPreferenceService()
    {
        IFileService fileService = new FakeFileService();
        PreferenceService preferenceService = new PreferenceService(fileService);
        return preferenceService;
    }

    [Fact]
    public void StorePreferenceItem_RightType_Succeeds()
    {
        PreferenceService preferenceService = CreateMockPreferenceService();
        PropertyInfo editPanelWidthProperty = typeof(UserPreferences).GetProperty(nameof(UserPreferences.EditPanelWidth))!;
        const double newFieldValue = 0;

        preferenceService.StorePreferenceItem(editPanelWidthProperty, newFieldValue);

        UserPreferences preferences = preferenceService.UserPreferenceData;
        Assert.Equal(newFieldValue, preferences.EditPanelWidth);
    }

    [Fact]
    public void StorePreferenceItem_WrongType_Throws()
    {
        PreferenceService preferenceService = CreateMockPreferenceService();
        PropertyInfo editPanelWidthProperty = typeof(UserPreferences).GetProperty(nameof(UserPreferences.EditPanelWidth))!;
        const string newFieldValue = "0";

        Action call = () => preferenceService.StorePreferenceItem(editPanelWidthProperty, newFieldValue);

        InvalidCastException e = Assert.Throws<InvalidCastException>(call);
        Assert.Equal(PreferenceService.PreferenceErrorCode, e.HResult);
    }

    [Fact]
    public void StorePreferenceItem_IncorrectObjectProperty_Throws()
    {
        PreferenceService preferenceService = CreateMockPreferenceService();
        PropertyInfo incorrectObjectProperty = typeof(string).GetProperty(nameof(string.Length))!;
        const int newFieldValue = 0;

        Action call = () => preferenceService.StorePreferenceItem(incorrectObjectProperty, newFieldValue);

        InvalidCastException e = Assert.Throws<InvalidCastException>(call);
        Assert.Equal(PreferenceService.PreferenceErrorCode, e.HResult);
    }


    [Fact]
    public void StorePreferenceDictionaryValue_RightTypes_Succeeds()
    {
        PreferenceService preferenceService = CreateMockPreferenceService();
        PropertyInfo columnWidthsProperty = typeof(UserPreferences).GetProperty(nameof(UserPreferences.ListColumnWidths))!;
        const string newKey = "TestKey";
        const double newValue = 0;

        preferenceService.StorePreferenceDictionaryValue(columnWidthsProperty, newKey, newValue);

        UserPreferences preferences = preferenceService.UserPreferenceData;
        Assert.Equal(newValue, preferences.ListColumnWidths[newKey]);
    }

    [Fact]
    public void StorePreferenceDictionaryValue_WrongKeyType_Throws()
    {
        PreferenceService preferenceService = CreateMockPreferenceService();
        PropertyInfo columnWidthsProperty = typeof(UserPreferences).GetProperty(nameof(UserPreferences.ListColumnWidths))!;
        const double newKey = 0;
        const string newValue = "0";

        Action call = () => preferenceService.StorePreferenceDictionaryValue(columnWidthsProperty, newKey, newValue);

        InvalidCastException e = Assert.Throws<InvalidCastException>(call);
        Assert.Equal(PreferenceService.PreferenceErrorCode, e.HResult);
    }

    [Fact]
    public void StorePreferenceDictionaryValue_WrongValueType_Throws()
    {
        PreferenceService preferenceService = CreateMockPreferenceService();
        PropertyInfo columnWidthsProperty = typeof(UserPreferences).GetProperty(nameof(UserPreferences.ListColumnWidths))!;
        const string newKey = "TestKey";
        const string newValue = "0";

        Action call = () => preferenceService.StorePreferenceDictionaryValue(columnWidthsProperty, newKey, newValue);

        InvalidCastException e = Assert.Throws<InvalidCastException>(call);
        Assert.Equal(PreferenceService.PreferenceErrorCode, e.HResult);
    }

    [Fact]
    public void StorePreferenceDictionaryValue_NotDictionary_Throws()
    {
        PreferenceService preferenceService = CreateMockPreferenceService();
        PropertyInfo incorrectTypeProperty = typeof(UserPreferences).GetProperty(nameof(UserPreferences.EditPanelWidth))!;
        const string newKey = "TestKey";
        const double newValue = 0;

        Action call = () => preferenceService.StorePreferenceDictionaryValue(incorrectTypeProperty, newKey, newValue);

        InvalidCastException e = Assert.Throws<InvalidCastException>(call);
        Assert.Equal(PreferenceService.PreferenceErrorCode, e.HResult);
    }

    [Fact]
    public void StorePreferenceDictionaryValue_IncorrectObjectProperty_Throws()
    {
        PreferenceService preferenceService = CreateMockPreferenceService();
        PropertyInfo incorrectObjectProperty = typeof(string).GetProperty(nameof(string.Length))!;
        const string newKey = "TestKey";
        const double newValue = 0;

        Action call = () => preferenceService.StorePreferenceDictionaryValue(incorrectObjectProperty, newKey, newValue);

        InvalidCastException e = Assert.Throws<InvalidCastException>(call);
        Assert.Equal(PreferenceService.PreferenceErrorCode, e.HResult);
    }

    [Fact]
    public void UserPreferences_ListColumnWidths_StartFull()
    {
        PreferenceService preferenceService = CreateMockPreferenceService();
        UserPreferences preferences = preferenceService.UserPreferenceData;

        Assert.NotEmpty(preferences.ListColumnWidths);
    }

    [Fact]
    public void StorePreferenceItem_SystemPreference_Succeeds()
    {
        PreferenceService preferenceService = CreateMockPreferenceService();
        PropertyInfo isMaximisedProperty = typeof(SystemPreferences).GetProperty(nameof(SystemPreferences.IsMaximised))!;

        preferenceService.StorePreferenceItem(isMaximisedProperty, true);

        Assert.True(preferenceService.SystemPreferenceData.IsMaximised);
    }

    [Fact]
    public async Task LoadPreferenceData_NoSavedData_LeavesListColumnWidthsFull()
    {
        PreferenceService preferenceService = CreateMockPreferenceService();

        await preferenceService.LoadPreferenceData();

        UserPreferences preferences = preferenceService.UserPreferenceData;
        Assert.NotEmpty(preferences.ListColumnWidths);
    }

    [Fact]
    public async Task LoadPreferenceData_WithSavedData_LoadsValues()
    {
        PreferenceService preferenceService = CreateMockPreferenceService();
        PropertyInfo editPanelWidthProperty = typeof(UserPreferences).GetProperty(nameof(UserPreferences.EditPanelWidth))!;
        const double newFieldValue = 0;
        const double changedFieldValue = 1;
        preferenceService.StorePreferenceItem(editPanelWidthProperty, newFieldValue);
        await preferenceService.SavePreferenceData();
        preferenceService.StorePreferenceItem(editPanelWidthProperty, changedFieldValue);

        await preferenceService.LoadPreferenceData();

        UserPreferences preferences = preferenceService.UserPreferenceData;
        Assert.Equal(newFieldValue, preferences.EditPanelWidth);
    }
}