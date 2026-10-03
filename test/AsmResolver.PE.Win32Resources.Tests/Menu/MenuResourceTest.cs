using System.Linq;
using AsmResolver.PE.Win32Resources.Menu;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.Menu;

public class MenuResourceTest
{
    private static MenuResource GetMenuResource(uint id, bool rebuild = false)
    {
        var image = PEImage.FromBytes(Properties.Resources.ResourceLibrary);
        var menus = MenuResource.FromDirectory(image.Resources!).ToList();

        if (rebuild)
        {
            foreach (var m in menus)
                m.InsertIntoDirectory(image.Resources!);
            menus = MenuResource.FromDirectory(image.Resources!).ToList();
        }

        return menus.First(m => m.Id == id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadMenu201IsStandard(bool rebuild)
    {
        var menu = GetMenuResource(201, rebuild);
        Assert.False(menu.IsExtended);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadMenu201HasThreeTopLevel(bool rebuild)
    {
        var menu = GetMenuResource(201, rebuild);
        Assert.Equal(3, menu.Items.Count);
        Assert.Equal("&File", menu.Items[0].Text);
        Assert.Equal("&Edit", menu.Items[1].Text);
        Assert.Equal("&Help", menu.Items[2].Text);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadMenu201FileHasItems(bool rebuild)
    {
        var menu = GetMenuResource(201, rebuild);
        var file = menu.Items[0];
        Assert.Equal(MenuItemType.Popup, file.Type);
        Assert.Equal(4, file.SubItems.Count);
        Assert.Equal("&New", file.SubItems[0].Text);
        Assert.Equal("&Open", file.SubItems[1].Text);
        Assert.Equal(MenuItemType.Separator, file.SubItems[2].Type);
        Assert.Equal("E&xit", file.SubItems[3].Text);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadMenu201ExitCommand(bool rebuild)
    {
        var menu = GetMenuResource(201, rebuild);
        var exit = menu.Items[0].SubItems[3];
        Assert.Equal(40003u, exit.Id);
        Assert.Equal(MenuItemType.Normal, exit.Type);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadMenu202IsExtended(bool rebuild)
    {
        var menu = GetMenuResource(202, rebuild);
        Assert.True(menu.IsExtended);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadMenu202ViewHasNestedZoom(bool rebuild)
    {
        var menu = GetMenuResource(202, rebuild);
        Assert.Equal(2, menu.Items.Count);

        var view = menu.Items[1];
        Assert.Equal("&View", view.Text);
        Assert.Equal(MenuItemType.Popup, view.Type);

        var zoom = view.SubItems.First(i => i.Text == "Z&oom");
        Assert.Equal(MenuItemType.Popup, zoom.Type);
        Assert.Equal(2, zoom.SubItems.Count);
        Assert.Equal("Zoom &In", zoom.SubItems[0].Text);
        Assert.Equal("Zoom &Out", zoom.SubItems[1].Text);
    }

    [Fact]
    public void RoundTripMenu201()
    {
        var original = GetMenuResource(201);
        var roundTripped = GetMenuResource(201, rebuild: true);

        Assert.Equal(original.IsExtended, roundTripped.IsExtended);
        Assert.Equal(original.Items.Count, roundTripped.Items.Count);
        AssertMenuItemsEqual(original.Items, roundTripped.Items);
    }

    [Fact]
    public void RoundTripMenu202()
    {
        var original = GetMenuResource(202);
        var roundTripped = GetMenuResource(202, rebuild: true);

        Assert.Equal(original.IsExtended, roundTripped.IsExtended);
        Assert.Equal(original.HelpId, roundTripped.HelpId);
        Assert.Equal(original.Items.Count, roundTripped.Items.Count);
        AssertMenuItemsEqual(original.Items, roundTripped.Items);
    }

    private static void AssertMenuItemsEqual(
        System.Collections.Generic.IList<MenuItem> expected,
        System.Collections.Generic.IList<MenuItem> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Type, actual[i].Type);
            Assert.Equal(expected[i].Text, actual[i].Text);
            Assert.Equal(expected[i].Id, actual[i].Id);
            AssertMenuItemsEqual(expected[i].SubItems, actual[i].SubItems);
        }
    }
}
