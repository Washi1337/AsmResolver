using System.Linq;
using AsmResolver.PE.Win32Resources.Bitmap;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.Bitmap;

public class BitmapResourceTest
{
    private static BitmapResource[] GetBitmapResources(bool rebuild = false)
    {
        var image = PEImage.FromBytes(Properties.Resources.ResourceLibrary);
        var bitmaps = BitmapResource.FromDirectory(image.Resources!).ToArray();

        if (rebuild)
        {
            var newResources = image.Resources!;
            foreach (var bmp in bitmaps)
                bmp.InsertIntoDirectory(newResources);

            bitmaps = BitmapResource.FromDirectory(newResources).ToArray();
        }

        return bitmaps;
    }

    [Fact]
    public void ReadBitmap101()
    {
        var bitmaps = GetBitmapResources();
        var bmp = bitmaps.First(b => b.Id == 101);

        Assert.Equal(16, bmp.Width);
        Assert.Equal(16, bmp.Height);
        Assert.Equal(24, bmp.BitCount);
        Assert.Equal(808u, bmp.Data.GetPhysicalSize());
    }

    [Fact]
    public void ReadBitmap102()
    {
        var bitmaps = GetBitmapResources();
        var bmp = bitmaps.First(b => b.Id == 102);

        Assert.Equal(32, bmp.Width);
        Assert.Equal(32, bmp.Height);
        Assert.Equal(24, bmp.BitCount);
        Assert.Equal(3112u, bmp.Data.GetPhysicalSize());
    }

    [Fact]
    public void ReadBitmapCount()
    {
        var bitmaps = GetBitmapResources();
        Assert.Equal(2, bitmaps.Length);
    }

    [Fact]
    public void RoundTrip()
    {
        var original = GetBitmapResources();
        var rebuilt = GetBitmapResources(rebuild: true);

        Assert.Equal(original.Length, rebuilt.Length);

        foreach (var origBmp in original)
        {
            var rebuildBmp = rebuilt.First(b => b.Id == origBmp.Id);
            Assert.Equal(origBmp.Data.GetPhysicalSize(), rebuildBmp.Data.GetPhysicalSize());
            Assert.Equal(origBmp.Width, rebuildBmp.Width);
            Assert.Equal(origBmp.Height, rebuildBmp.Height);
            Assert.Equal(origBmp.BitCount, rebuildBmp.BitCount);
        }
    }
}
