
namespace FileTagger.Statics;

public static class Consts
{
    /// <summary>
    /// Value for edit fields that we don't want to change
    /// </summary>
    public const string UnchangedField = "< keep >";

    /// <summary>
    /// Name of the app, used in the window titlebar and the preferences save location
    /// </summary>
    public const string AppName = "NetTag";
}


public static class Maths
{
    public static int ChangeCollectionIndex(int currentIndex, int collectionCount, int change)
    {
        int newIndex = currentIndex + change;
        if (newIndex >= collectionCount) return newIndex % collectionCount;
        if (newIndex < 0) return newIndex + collectionCount;
        return newIndex;
    }
}