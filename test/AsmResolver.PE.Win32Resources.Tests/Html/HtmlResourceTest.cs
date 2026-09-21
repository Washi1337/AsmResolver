using System.Linq;
using AsmResolver.PE.Win32Resources.Html;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.Html;

public class HtmlResourceTest
{
    private static HtmlResource[] GetTestHtmlResources(bool rebuild)
    {
        var image = PEImage.FromBytes(Properties.Resources.ResourceLibrary);
        var resources = HtmlResource.FromDirectory(image.Resources!).ToArray();

        if (rebuild)
        {
            foreach (var resource in resources)
                resource.InsertIntoDirectory(image.Resources!);

            resources = HtmlResource.FromDirectory(image.Resources!).ToArray();
        }

        return resources;
    }

    [Fact]
    public void ReadCount()
    {
        var resources = GetTestHtmlResources(false);
        Assert.Equal(2, resources.Length);
    }

    [Fact]
    public void ReadHtml501Content()
    {
        var resources = GetTestHtmlResources(false);
        var html501 = resources.First(r => r.Id == 501);
        Assert.Contains("<html>", html501.Content);
    }

    [Fact]
    public void ReadHtml502Content()
    {
        var resources = GetTestHtmlResources(false);
        var html502 = resources.First(r => r.Id == 502);
        Assert.Contains("\u00e4\u00f6\u00fc \u00f1", html502.Content);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RoundTrip(bool rebuild)
    {
        var resources = GetTestHtmlResources(rebuild);

        Assert.Equal(2, resources.Length);

        var html501 = resources.First(r => r.Id == 501);
        Assert.Contains("<html>", html501.Content);

        var html502 = resources.First(r => r.Id == 502);
        Assert.Contains("\u00e4\u00f6\u00fc \u00f1", html502.Content);
    }
}
