using System.Linq;
using AsmResolver.PE.Win32Resources.AnimatedCursor;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.AnimatedCursor;

public class AnimatedCursorResourceTest
{
    private static PEImage GetTestImage()
    {
        return PEImage.FromBytes(Properties.Resources.ResourceLibrary);
    }

    [Fact]
    public void ReadAniCursor601()
    {
        var image = GetTestImage();
        var cursors = AnimatedCursorResource.FromDirectory(image.Resources!).ToList();
        var cursor = cursors.First(c => c.Id == 601);
        Assert.Equal(4362u, cursor.RawData.GetPhysicalSize());
        Assert.Equal(1u, cursor.FrameCount);
        Assert.Equal(8u, cursor.DisplayRate);
    }

    [Fact]
    public void ReadAniIcon701()
    {
        var image = GetTestImage();
        var icons = AnimatedCursorResource.FromDirectory(image.Resources!, ResourceType.AniIcon).ToList();
        var icon = icons.First(i => i.Id == 701);
        Assert.Equal(4362u, icon.RawData.GetPhysicalSize());
    }

    [Fact]
    public void ReadAniCursorHasIconFrames()
    {
        var image = GetTestImage();
        var cursors = AnimatedCursorResource.FromDirectory(image.Resources!).ToList();
        var cursor = cursors.First(c => c.Id == 601);
        Assert.True(cursor.HasIconFrames);
    }

    [Fact]
    public void RoundTrip()
    {
        var image = GetTestImage();
        var cursors = AnimatedCursorResource.FromDirectory(image.Resources!).ToList();
        var icons = AnimatedCursorResource.FromDirectory(image.Resources!, ResourceType.AniIcon).ToList();

        // Re-insert all.
        foreach (var cursor in cursors)
            cursor.InsertIntoDirectory(image.Resources!);
        foreach (var icon in icons)
            icon.InsertIntoDirectory(image.Resources!);

        // Re-read and verify.
        var cursors2 = AnimatedCursorResource.FromDirectory(image.Resources!).ToList();
        var icons2 = AnimatedCursorResource.FromDirectory(image.Resources!, ResourceType.AniIcon).ToList();

        Assert.Equal(cursors.Count, cursors2.Count);
        Assert.Equal(icons.Count, icons2.Count);

        foreach (var original in cursors)
        {
            var roundTripped = cursors2.First(c => c.Id == original.Id);
            Assert.Equal(original.RawData.GetPhysicalSize(), roundTripped.RawData.GetPhysicalSize());
        }

        foreach (var original in icons)
        {
            var roundTripped = icons2.First(i => i.Id == original.Id);
            Assert.Equal(original.RawData.GetPhysicalSize(), roundTripped.RawData.GetPhysicalSize());
        }
    }
}
