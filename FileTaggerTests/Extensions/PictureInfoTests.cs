using ATL;
using FileTagger.Extensions;

namespace FileTaggerTests.Extensions;

public class PictureInfoTests
{
    private static PictureInfo CreateMockPicture(PictureInfo.PIC_TYPE type, string description)
    {
        return new PictureInfo(type) { Description = description };
    }

    [Theory]
    [InlineData("Front cover", 1, PictureInfo.PIC_TYPE.Front)]
    [InlineData("Back cover", 1, PictureInfo.PIC_TYPE.Front)]
    [InlineData("Front cover", 2, PictureInfo.PIC_TYPE.Front)]
    [InlineData("Front cover", 1, PictureInfo.PIC_TYPE.Back)]
    public void PicturesEqual_SameHash_AlwaysTrue(string description, int position, PictureInfo.PIC_TYPE type)
    {
        PictureInfo firstPicture = CreateMockPicture(PictureInfo.PIC_TYPE.Front, "Front cover");
        PictureInfo secondPicture = CreateMockPicture(type, description);
        secondPicture.Position = position;

        Assert.True(firstPicture.PicturesEqual(secondPicture));
    }

    [Fact]
    public void PicturesEqual_DifferentHash_AlwaysFalse()
    {
        PictureInfo firstPicture = CreateMockPicture(PictureInfo.PIC_TYPE.Front, "");
        firstPicture.PictureHash = 1;
        PictureInfo secondPicture = CreateMockPicture(PictureInfo.PIC_TYPE.Front, "");

        Assert.False(firstPicture.PicturesEqual(secondPicture));
    }

    [Theory]
    [InlineData("Front cover", 1, PictureInfo.PIC_TYPE.Front)]
    [InlineData("Back cover", 1, PictureInfo.PIC_TYPE.Front)]
    public void TrueEqual_DifferentDescription_AlwaysTrue(string description, int position, PictureInfo.PIC_TYPE type)
    {
        PictureInfo firstPicture = CreateMockPicture(PictureInfo.PIC_TYPE.Front, "Front cover");
        PictureInfo secondPicture = CreateMockPicture(type, description);
        secondPicture.Position = position;

        Assert.True(firstPicture.TrueEqual(secondPicture));
    }

    [Theory]
    [InlineData(1, 1, PictureInfo.PIC_TYPE.Front)]
    [InlineData(2, 0, PictureInfo.PIC_TYPE.Front)]
    [InlineData(1, 0, PictureInfo.PIC_TYPE.Back)]
    [InlineData(2, 1, PictureInfo.PIC_TYPE.Back)]
    public void TrueEqual_DifferentPositionHashOrPicType_AlwaysFalse(int position, uint hash, PictureInfo.PIC_TYPE type)
    {
        PictureInfo firstPicture = CreateMockPicture(PictureInfo.PIC_TYPE.Front, "");
        PictureInfo secondPicture = CreateMockPicture(type, "");
        secondPicture.Position = position;
        secondPicture.PictureHash = hash;

        Assert.False(firstPicture.TrueEqual(secondPicture));
    }
}