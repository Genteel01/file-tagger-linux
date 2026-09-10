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
        PropertyInfo editPanelWidthProperty = typeof(Preferences).GetProperty(nameof(Preferences.EditPanelWidth))!;
        const double newFieldValue = 0;

        preferenceService.StorePreferenceItem(editPanelWidthProperty, newFieldValue);

        Preferences preferences = preferenceService.GetPreferenceData();
        Assert.Equal(newFieldValue, preferences.EditPanelWidth);
    }

    [Fact]
    public void StorePreferenceItem_WrongType_Throws()
    {
        PreferenceService preferenceService = CreateMockPreferenceService();
        PropertyInfo editPanelWidthProperty = typeof(Preferences).GetProperty(nameof(Preferences.EditPanelWidth))!;
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

        Assert.ThrowsAny<Exception>(call);
    }
}