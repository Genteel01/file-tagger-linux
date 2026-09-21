using FileTagger.Assets.Statics;
using FileTagger.Services;
using FileTagger.ViewModels;
using FileTaggerTests.Fakers;

namespace FileTaggerTests.ViewModels;

public class EditPanelViewModelTests
{
    private static EditPanelViewModel CreateMockViewModel()
    {
        IFileService fileService = new FakeFileService();
        IImageService imageService = new FakeImageService();
        EditPanelViewModel editPanelViewModel = new EditPanelViewModel(fileService, imageService);
        return editPanelViewModel;
    }

    [Fact]
    public void ReceiveMessage_NoTracks_NoError()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        MainWindowViewModel.SelectedItemsMessage message = new MainWindowViewModel.SelectedItemsMessage([]);

        viewModel.Receive(message);

        Assert.Empty(viewModel.SelectedTracks);
    }

    [Fact]
    public void ReceiveMessage_WithTracks_Assigned()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        List<TrackViewModel> stubTracks = [StubCreators.CreateStubTrackViewModel()];
        MainWindowViewModel.SelectedItemsMessage message = new MainWindowViewModel.SelectedItemsMessage(stubTracks);

        viewModel.Receive(message);

        Assert.Equal(stubTracks.Count, viewModel.SelectedTracks.Count);
        Assert.Equal(stubTracks[0], viewModel.SelectedTracks[0]);
        Assert.Equal(stubTracks, viewModel.SelectedTracks);
    }

    [Fact]
    public void ReceiveMessage_NoTracks_FieldTextsAreEmptyAndFieldOptionsAreEmpty()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([]));

        Assert.All(viewModel.FieldTexts.Values, value => Assert.Equal("", value));
        Assert.All(viewModel.FieldOptions.Values, options => Assert.Empty(options));
    }

    [Fact]
    public void ReceiveMessage_OneTrack_StringFieldUsesValue()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.Artist);
        TrackViewModel track = StubCreators.CreateStubTrackViewModel();
        const string artistNameString = "Artist";
        track.Artist = artistNameString;

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([track]));

        Assert.Equal(artistNameString, viewModel.FieldTexts[fieldName]);
        Assert.Equal(
            new object?[] { Consts.UnchangedField, artistNameString },
            viewModel.FieldOptions[fieldName]);
    }

    [Fact]
    public void ReceiveMessage_OneTrack_StringFieldExcludesEmptyOption()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.Artist);
        TrackViewModel track = StubCreators.CreateStubTrackViewModel();
        const string artistNameString = "";
        track.Artist = artistNameString;

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([track]));

        Assert.Equal(artistNameString, viewModel.FieldTexts[fieldName]);
        Assert.Equal(
            new object?[] { Consts.UnchangedField },
            viewModel.FieldOptions[fieldName]);
    }

    [Fact]
    public void ReceiveMessage_OneTrack_IntFieldUsesValue()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.TrackNumber);
        TrackViewModel track = StubCreators.CreateStubTrackViewModel();
        const int trackNumber = 7;
        track.TrackNumber = trackNumber;

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([track]));

        Assert.Equal(trackNumber.ToString(), viewModel.FieldTexts[fieldName]);
        Assert.Equal(
            new object?[] { Consts.UnchangedField, trackNumber },
            viewModel.FieldOptions[fieldName]);
    }

    [Fact]
    public void ReceiveMessage_OneTrack_IntFieldExcludesNullOption()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.TrackNumber);
        TrackViewModel track = StubCreators.CreateStubTrackViewModel();
        track.TrackNumber = null;

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([track]));

        Assert.Equal("", viewModel.FieldTexts[fieldName]);
        Assert.Equal(
            new object?[] { Consts.UnchangedField },
            viewModel.FieldOptions[fieldName]);
    }

    [Fact]
    public void ReceiveMessage_MultipleTracksWithMatchingStringValues_UsesValueAsFieldText()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.Artist);
        TrackViewModel firstTrack = StubCreators.CreateStubTrackViewModel();
        TrackViewModel secondTrack = StubCreators.CreateStubTrackViewModel();
        const string artistNameString = "Artist";
        firstTrack.Artist = secondTrack.Artist = artistNameString;

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([firstTrack, secondTrack]));

        Assert.Equal(artistNameString, viewModel.FieldTexts[fieldName]);
        Assert.Equal(
            new object?[] { Consts.UnchangedField, artistNameString },
            viewModel.FieldOptions[fieldName]);

    }

    [Fact]
    public void ReceiveMessage_MultipleTracksWithMatchingIntValues_UsesValueAsFieldText()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.TrackNumber);
        TrackViewModel firstTrack = StubCreators.CreateStubTrackViewModel();
        TrackViewModel secondTrack = StubCreators.CreateStubTrackViewModel();
        const int trackNumber = 7;
        firstTrack.TrackNumber = secondTrack.TrackNumber = trackNumber;

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([firstTrack, secondTrack]));

        Assert.Equal(trackNumber.ToString(), viewModel.FieldTexts[fieldName]);
        Assert.Equal(
            new object?[] { Consts.UnchangedField, trackNumber },
            viewModel.FieldOptions[fieldName]);
    }

    [Fact]
    public void ReceiveMessage_MultipleTracksWithDifferentStringValues_UsesKeepAsFieldText()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.Artist);
        TrackViewModel firstTrack = StubCreators.CreateStubTrackViewModel();
        TrackViewModel secondTrack = StubCreators.CreateStubTrackViewModel();
        const string firstArtistName = "Artist 1";
        const string secondArtistName = "Artist 2";
        firstTrack.Artist = firstArtistName;
        secondTrack.Artist = secondArtistName;

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([firstTrack, secondTrack]));

        Assert.Equal(Consts.UnchangedField, viewModel.FieldTexts[fieldName]);
        Assert.Equal(
            new object?[] { Consts.UnchangedField, firstArtistName, secondArtistName },
            viewModel.FieldOptions[fieldName]);

    }

    [Fact]
    public void ReceiveMessage_MultipleTracksWithDifferentIntValues_UsesKeepAsFieldText()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.TrackNumber);
        TrackViewModel firstTrack = StubCreators.CreateStubTrackViewModel();
        TrackViewModel secondTrack = StubCreators.CreateStubTrackViewModel();
        const int firstTrackNumber = 1;
        const int secondTrackNumber = 2;
        firstTrack.TrackNumber = firstTrackNumber;
        secondTrack.TrackNumber = secondTrackNumber;

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([firstTrack, secondTrack]));

        Assert.Equal(Consts.UnchangedField, viewModel.FieldTexts[fieldName]);
        Assert.Equal(
            new object?[] { Consts.UnchangedField, firstTrackNumber, secondTrackNumber },
            viewModel.FieldOptions[fieldName]);
    }

    [Fact]
    public void ReceiveMessage_MultipleTracksWithEmptyStringValue_UsesKeepAsFieldTextAndExcludesBlankOption()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.Artist);
        TrackViewModel firstTrack = StubCreators.CreateStubTrackViewModel();
        TrackViewModel secondTrack = StubCreators.CreateStubTrackViewModel();
        const string firstArtistName = "";
        const string secondArtistName = "Artist 2";
        firstTrack.Artist = firstArtistName;
        secondTrack.Artist = secondArtistName;

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([firstTrack, secondTrack]));

        Assert.Equal(Consts.UnchangedField, viewModel.FieldTexts[fieldName]);
        Assert.Equal(
            new object?[] { Consts.UnchangedField, secondArtistName },
            viewModel.FieldOptions[fieldName]);

    }

    [Fact]
    public void ReceiveMessage_MultipleTracksWithNullIntValue_UsesKeepAsFieldTextAndExcludesNullOption()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.TrackNumber);
        TrackViewModel firstTrack = StubCreators.CreateStubTrackViewModel();
        TrackViewModel secondTrack = StubCreators.CreateStubTrackViewModel();
        int? firstTrackNumber = null;
        const int secondTrackNumber = 2;
        firstTrack.TrackNumber = firstTrackNumber;
        secondTrack.TrackNumber = secondTrackNumber;

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([firstTrack, secondTrack]));

        Assert.Equal(Consts.UnchangedField, viewModel.FieldTexts[fieldName]);
        Assert.Equal(
            new object?[] { Consts.UnchangedField, secondTrackNumber },
            viewModel.FieldOptions[fieldName]);
    }

    [Fact]
    public void ReceiveMessage_Multiple_Tracks_StringFieldExcludesEmptyOption()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.Artist);
        TrackViewModel firstTrack = StubCreators.CreateStubTrackViewModel();
        TrackViewModel secondTrack = StubCreators.CreateStubTrackViewModel();
        const string artistName = "";
        firstTrack.Artist = artistName;
        secondTrack.Artist = artistName;

        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([firstTrack, secondTrack]));

        Assert.Equal(artistName, viewModel.FieldTexts[fieldName]);
        Assert.Equal(
            new object?[] { Consts.UnchangedField },
            viewModel.FieldOptions[fieldName]);
    }

    [Fact]
    public void ReceiveMessage_Multiple_Tracks_IntFieldExcludesNullOption()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.TrackNumber);
        TrackViewModel firstTrack = StubCreators.CreateStubTrackViewModel();
        TrackViewModel secondTrack = StubCreators.CreateStubTrackViewModel();
        int? trackNumber = null;
        firstTrack.TrackNumber = trackNumber;
        secondTrack.TrackNumber = trackNumber;


        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([firstTrack, secondTrack]));

        Assert.Equal("", viewModel.FieldTexts[fieldName]);
        Assert.Equal(
            new object?[] { Consts.UnchangedField },
            viewModel.FieldOptions[fieldName]);
    }

    [Fact]
    public void StoreFieldChanges_StringFieldWithValue_StoresValue()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.Artist);
        TrackViewModel track = StubCreators.CreateStubTrackViewModel();
        const string updatedString = "Updated";
        track.Artist = "Original";
        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([track]));
        viewModel.FieldTexts[fieldName] = updatedString;

        viewModel.StoreFieldChanges();

        Assert.Equal(updatedString, track.Artist);
    }

    [Fact]
    public void StoreFieldChanges_StringFieldWithUnchangedField_KeepsValue()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.Artist);
        TrackViewModel track = StubCreators.CreateStubTrackViewModel();
        const string fieldValue = "Original";
        track.Artist = fieldValue;
        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([track]));
        viewModel.FieldTexts[fieldName] = Consts.UnchangedField;

        viewModel.StoreFieldChanges();

        Assert.Equal(fieldValue, track.Artist);
    }

    [Fact]
    public void StoreFieldChanges_IntFieldWithValue_StoresValue()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.TrackNumber);
        TrackViewModel track = StubCreators.CreateStubTrackViewModel();
        track.TrackNumber = 1;
        const int updatedValue = 7;
        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([track]));
        viewModel.FieldTexts[fieldName] = updatedValue.ToString();

        viewModel.StoreFieldChanges();

        Assert.Equal(updatedValue, track.TrackNumber);
    }

    [Fact]
    public void StoreFieldChanges_IntFieldWithNullValue_ClearsValue()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.TrackNumber);
        TrackViewModel track = StubCreators.CreateStubTrackViewModel();
        track.TrackNumber = 1;
        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([track]));
        viewModel.FieldTexts[fieldName] = "";

        viewModel.StoreFieldChanges();

        Assert.Null(track.TrackNumber);
    }

    [Fact]
    public void StoreFieldChanges_IntFieldWithUnchangedField_KeepsValue()
    {
        EditPanelViewModel viewModel = CreateMockViewModel();
        const string fieldName = nameof(TrackViewModel.TrackNumber);
        TrackViewModel track = StubCreators.CreateStubTrackViewModel();
        const int fieldValue = 1;
        track.TrackNumber = fieldValue;
        viewModel.Receive(new MainWindowViewModel.SelectedItemsMessage([track]));
        viewModel.FieldTexts[fieldName] = Consts.UnchangedField;

        viewModel.StoreFieldChanges();

        Assert.Equal(fieldValue, track.TrackNumber);
    }
}