using System.Linq;
using AsmResolver.PE.Win32Resources.DialogInclude;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.DialogInclude;

public class DialogIncludeResourceTest
{
    private static DialogIncludeResource[] GetTestDialogIncludeResources(bool rebuild)
    {
        var image = PEImage.FromBytes(Properties.Resources.ResourceLibrary);
        var resources = DialogIncludeResource.FromDirectory(image.Resources!).ToArray();

        if (rebuild)
        {
            foreach (var resource in resources)
                resource.InsertIntoDirectory(image.Resources!);

            resources = DialogIncludeResource.FromDirectory(image.Resources!).ToArray();
        }

        return resources;
    }

    [Fact]
    public void ReadHeaderFileName()
    {
        var resources = GetTestDialogIncludeResources(false);
        var dlgInclude = resources.First(r => r.Id == 1);
        Assert.Contains("#define IDC_STATIC", dlgInclude.HeaderFileName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RoundTrip(bool rebuild)
    {
        var resources = GetTestDialogIncludeResources(rebuild);
        var dlgInclude = resources.First(r => r.Id == 1);
        Assert.Contains("#define IDC_STATIC", dlgInclude.HeaderFileName);
    }
}
