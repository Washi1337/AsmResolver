using System.Linq;
using AsmResolver.PE.Win32Resources.Font;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.Font;

public class FontDirectoryResourceTest
{
    private static PEImage GetTestImage()
    {
        return PEImage.FromBytes(Properties.Resources.ResourceLibrary);
    }

    [Fact]
    public void ReadEntryCount()
    {
        var image = GetTestImage();
        var fontDir = FontDirectoryResource.FromDirectory(image.Resources!);
        Assert.NotNull(fontDir);
        Assert.Equal(2, fontDir.Entries.Count);
    }

    [Fact]
    public void ReadEntry1Ordinal()
    {
        var image = GetTestImage();
        var fontDir = FontDirectoryResource.FromDirectory(image.Resources!);
        Assert.NotNull(fontDir);
        Assert.Equal((ushort)1, fontDir.Entries[0].FontOrdinal);
    }

    [Fact]
    public void ReadEntry2Ordinal()
    {
        var image = GetTestImage();
        var fontDir = FontDirectoryResource.FromDirectory(image.Resources!);
        Assert.NotNull(fontDir);
        Assert.Equal((ushort)2, fontDir.Entries[1].FontOrdinal);
    }

    [Fact]
    public void RoundTrip()
    {
        var image = GetTestImage();
        var fontDir = FontDirectoryResource.FromDirectory(image.Resources!);
        Assert.NotNull(fontDir);

        // Re-insert.
        fontDir.InsertIntoDirectory(image.Resources!);

        // Re-read and verify.
        var fontDir2 = FontDirectoryResource.FromDirectory(image.Resources!);
        Assert.NotNull(fontDir2);
        Assert.Equal(fontDir.Entries.Count, fontDir2.Entries.Count);

        for (int i = 0; i < fontDir.Entries.Count; i++)
        {
            Assert.Equal(fontDir.Entries[i].FontOrdinal, fontDir2.Entries[i].FontOrdinal);
            Assert.Equal(
                fontDir.Entries[i].RawData.GetPhysicalSize(),
                fontDir2.Entries[i].RawData.GetPhysicalSize());
        }
    }
}
