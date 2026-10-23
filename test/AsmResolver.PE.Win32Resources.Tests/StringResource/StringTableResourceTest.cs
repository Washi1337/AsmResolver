using System.Linq;
using AsmResolver.PE.Win32Resources.StringResource;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.StringResource;

public class StringTableResourceTest
{
    private static StringTableResource GetStringTableResource(bool rebuild = false)
    {
        var image = PEImage.FromBytes(Properties.Resources.ResourceLibrary);
        var resource = StringTableResource.FromDirectory(image.Resources!);
        Assert.NotNull(resource);

        if (rebuild)
        {
            resource.InsertIntoDirectory(image.Resources!);
            resource = StringTableResource.FromDirectory(image.Resources!);
            Assert.NotNull(resource);
        }

        return resource;
    }

    [Fact]
    public void ReadStringById()
    {
        var resource = GetStringTableResource();
        Assert.Equal("Hello", resource.Strings[1]);
        Assert.Equal("World", resource.Strings[2]);
        Assert.Equal("AsmResolver Test String", resource.Strings[3]);
    }

    [Fact]
    public void ReadStringInSecondBlock()
    {
        var resource = GetStringTableResource();
        Assert.Equal("Second block string", resource.Strings[16]);
    }

    [Fact]
    public void ReadSparseBlock()
    {
        var resource = GetStringTableResource();
        Assert.Equal("Sparse block string", resource.Strings[48]);
    }

    [Fact]
    public void RoundTrip()
    {
        var original = GetStringTableResource();
        var rebuilt = GetStringTableResource(rebuild: true);

        Assert.Equal(original.Strings.Count, rebuilt.Strings.Count);
        foreach (var kvp in original.Strings)
        {
            Assert.True(rebuilt.Strings.ContainsKey(kvp.Key), $"Missing string ID {kvp.Key}");
            Assert.Equal(kvp.Value, rebuilt.Strings[kvp.Key]);
        }
    }
}
