using System.Linq;
using AsmResolver.PE.Win32Resources.MessageTable;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.MessageTable;

public class MessageTableResourceTest
{
    private static MessageTableResource GetTestMessageTableResource(bool rebuild)
    {
        var image = PEImage.FromBytes(Properties.Resources.ResourceLibrary);
        var resource = MessageTableResource.FromDirectory(image.Resources!);
        Assert.NotNull(resource);

        if (rebuild)
        {
            resource.InsertIntoDirectory(image.Resources!);
            resource = MessageTableResource.FromDirectory(image.Resources!);
            Assert.NotNull(resource);
        }

        return resource;
    }

    [Fact]
    public void ReadMessageCount()
    {
        var resource = GetTestMessageTableResource(false);
        Assert.Equal(2, resource.Entries.Count);
    }

    [Fact]
    public void ReadMessage1()
    {
        var resource = GetTestMessageTableResource(false);
        var entry = resource.Entries.First(e => e.Id == 1);
        Assert.Contains("Hello, World!", entry.Text);
    }

    [Fact]
    public void ReadMessage2()
    {
        var resource = GetTestMessageTableResource(false);
        var entry = resource.Entries.First(e => e.Id == 2);
        Assert.Contains("Goodbye, World!", entry.Text);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RoundTrip(bool rebuild)
    {
        var resource = GetTestMessageTableResource(rebuild);

        Assert.Equal(2, resource.Entries.Count);

        var msg1 = resource.Entries.First(e => e.Id == 1);
        Assert.Contains("Hello, World!", msg1.Text);

        var msg2 = resource.Entries.First(e => e.Id == 2);
        Assert.Contains("Goodbye, World!", msg2.Text);
    }
}
