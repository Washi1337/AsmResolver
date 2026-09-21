using System.Linq;
using AsmResolver.PE.Win32Resources.Font;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.Font;

public class FontResourceTest
{
    private static PEImage GetTestImage()
    {
        return PEImage.FromBytes(Properties.Resources.ResourceLibrary);
    }

    [Fact]
    public void ReadFontCount()
    {
        var image = GetTestImage();
        var fonts = FontResource.FromDirectory(image.Resources!).ToList();
        Assert.Equal(2, fonts.Count);
    }

    [Fact]
    public void ReadFont5Size()
    {
        var image = GetTestImage();
        var fonts = FontResource.FromDirectory(image.Resources!).ToList();
        var font5 = fonts.First(f => f.Id == 5);
        Assert.Equal(4464u, font5.Data.GetPhysicalSize());
    }

    [Fact]
    public void ReadFont6Size()
    {
        var image = GetTestImage();
        var fonts = FontResource.FromDirectory(image.Resources!).ToList();
        var font6 = fonts.First(f => f.Id == 6);
        Assert.Equal(6064u, font6.Data.GetPhysicalSize());
    }

    [Fact]
    public void RoundTrip()
    {
        var image = GetTestImage();
        var fonts = FontResource.FromDirectory(image.Resources!).ToList();

        // Re-insert all fonts.
        foreach (var font in fonts)
            font.InsertIntoDirectory(image.Resources!);

        // Re-read and verify.
        var fonts2 = FontResource.FromDirectory(image.Resources!).ToList();
        Assert.Equal(fonts.Count, fonts2.Count);

        foreach (var original in fonts)
        {
            var roundTripped = fonts2.First(f => f.Id == original.Id);
            Assert.Equal(original.Data.GetPhysicalSize(), roundTripped.Data.GetPhysicalSize());
        }
    }
}
