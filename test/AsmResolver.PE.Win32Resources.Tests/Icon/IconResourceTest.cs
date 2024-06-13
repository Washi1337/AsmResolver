using System.Linq;
using AsmResolver.PE.Builder;
using AsmResolver.PE.Win32Resources.Icon;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.Icon;

public class IconResourceTest : IClassFixture<TemporaryDirectoryFixture>
{
    private readonly TemporaryDirectoryFixture _fixture;

    public IconResourceTest(TemporaryDirectoryFixture fixture)
    {
        _fixture = fixture;
    }

    private static IconResource GetTestIconResource(bool rebuild)
    {
        // Load dummy.
        var image = PEImage.FromBytes(Properties.Resources.HelloWorld);

        // Read icon resources.
        var iconResource = IconResource.FromDirectory(image.Resources!, IconType.Icon);
        Assert.NotNull(iconResource);

        if (rebuild)
        {
            iconResource.InsertIntoDirectory(image.Resources!);
            iconResource = IconResource.FromDirectory(image.Resources!, IconType.Icon);
        }

        return iconResource;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IconGroup(bool rebuild)
    {
        var iconResource = GetTestIconResource(rebuild);

        // Verify.
        var group = Assert.Single(iconResource.Groups);
        Assert.Equal(32512u, group.Id);
        Assert.Equal(0u, group.Lcid);
        Assert.Equal(IconType.Icon, group.Type);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IconEntries(bool rebuild)
    {
        var iconResource = GetTestIconResource(rebuild);

        // Verify.
        var icons = iconResource.GetGroup(32512).Icons;
        Assert.Equal([1, 2, 3, 4], icons.Select(x => x.Id));
        Assert.Equal([16, 32, 48, 64], icons.Select(x => x.Width));
        Assert.Equal([16, 32, 48, 64], icons.Select(x => x.Height));
        Assert.All(icons, icon => Assert.Equal(32, icon.BitsPerPixel));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IconData(bool rebuild)
    {
        var iconResource = GetTestIconResource(rebuild);

        // Verify.
        var icons = iconResource.GetGroup(32512).Icons;
        Assert.All(icons, icon => Assert.NotNull(icon.PixelData));
    }

    [Fact]
    public void AddNewIcons()
    {
        var image = PEImage.FromBytes(Properties.Resources.HelloWorld);
        var iconResource = IconResource.FromDirectory(image.Resources!, IconType.Icon)!;

        var iconGroup = new IconGroup(1337, 1033);
        iconGroup.Icons.Add(new IconEntry(100, 1033)
        {
            Width = 10,
            Height = 10,
            BitsPerPixel = 32,
            PixelData = new DataSegment([1, 2, 3, 4]),
        });

        iconResource.Groups.Add(iconGroup);
        iconResource.InsertIntoDirectory(image.Resources!);

        var newIconResource = IconResource.FromDirectory(image.Resources!, IconType.Icon);
        Assert.NotNull(newIconResource);

        Assert.Equal([32512, 1337], newIconResource.Groups.Select(x => x.Id));
    }

    [Fact]
    public void RebuildPEWithIconResource()
    {
        // Load dummy.
        var image = PEImage.FromBytes(Properties.Resources.HelloWorld);

        // Rebuild icon resources.
        var iconResource = IconResource.FromDirectory(image.Resources!, IconType.Icon)!;
        iconResource.InsertIntoDirectory(image.Resources!);

        var file = image.ToPEFile(new ManagedPEFileBuilder());
        _fixture
            .GetRunner<FrameworkPERunner>()
            .RebuildAndRun(file, "HelloWorld.exe", "Hello World!\n");
    }
}
