using System.Linq;
using AsmResolver.PE.Win32Resources.Dialog;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.Dialog;

public class DialogResourceTest
{
    private static DialogResource GetDialogResource(uint id, bool rebuild = false)
    {
        var image = PEImage.FromBytes(Properties.Resources.ResourceLibrary);
        var dialogs = DialogResource.FromDirectory(image.Resources!).ToList();

        if (rebuild)
        {
            foreach (var d in dialogs)
                d.InsertIntoDirectory(image.Resources!);
            dialogs = DialogResource.FromDirectory(image.Resources!).ToList();
        }

        return dialogs.First(d => d.Id == id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadDialog301IsStandard(bool rebuild)
    {
        var dialog = GetDialogResource(301, rebuild);
        Assert.False(dialog.IsExtended);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadDialog301Caption(bool rebuild)
    {
        var dialog = GetDialogResource(301, rebuild);
        Assert.Equal("Simple Dialog", dialog.Caption);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadDialog301HasNoFont(bool rebuild)
    {
        var dialog = GetDialogResource(301, rebuild);
        Assert.Null(dialog.FontName);
        Assert.Null(dialog.FontSize);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadDialog301Controls(bool rebuild)
    {
        var dialog = GetDialogResource(301, rebuild);
        Assert.Equal(3, dialog.Controls.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadDialog302IsExtended(bool rebuild)
    {
        var dialog = GetDialogResource(302, rebuild);
        Assert.True(dialog.IsExtended);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadDialog302Caption(bool rebuild)
    {
        var dialog = GetDialogResource(302, rebuild);
        Assert.Equal("Extended Dialog", dialog.Caption);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadDialog302Font(bool rebuild)
    {
        var dialog = GetDialogResource(302, rebuild);
        Assert.Equal("Segoe UI", dialog.FontName);
        Assert.Equal((ushort)9, dialog.FontSize);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadDialog302Controls(bool rebuild)
    {
        var dialog = GetDialogResource(302, rebuild);
        Assert.Equal(10, dialog.Controls.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadDialog302HasComboBox(bool rebuild)
    {
        var dialog = GetDialogResource(302, rebuild);
        // CBS_DROPDOWNLIST = 0x0003, combobox class ordinal = 133
        var combo = dialog.Controls.FirstOrDefault(c => c.ClassOrdinal == 133);
        Assert.NotNull(combo);
        Assert.Equal(0x50200003u, combo.Style);
    }

    [Fact]
    public void RoundTripDialog301()
    {
        var original = GetDialogResource(301);
        var roundTripped = GetDialogResource(301, rebuild: true);

        Assert.Equal(original.IsExtended, roundTripped.IsExtended);
        Assert.Equal(original.Caption, roundTripped.Caption);
        Assert.Equal(original.Style, roundTripped.Style);
        Assert.Equal(original.Controls.Count, roundTripped.Controls.Count);

        for (int i = 0; i < original.Controls.Count; i++)
        {
            Assert.Equal(original.Controls[i].Id, roundTripped.Controls[i].Id);
            Assert.Equal(original.Controls[i].Style, roundTripped.Controls[i].Style);
            Assert.Equal(original.Controls[i].ClassOrdinal, roundTripped.Controls[i].ClassOrdinal);
            Assert.Equal(original.Controls[i].Text, roundTripped.Controls[i].Text);
        }
    }

    [Fact]
    public void RoundTripDialog302()
    {
        var original = GetDialogResource(302);
        var roundTripped = GetDialogResource(302, rebuild: true);

        Assert.Equal(original.IsExtended, roundTripped.IsExtended);
        Assert.Equal(original.Caption, roundTripped.Caption);
        Assert.Equal(original.Style, roundTripped.Style);
        Assert.Equal(original.FontName, roundTripped.FontName);
        Assert.Equal(original.FontSize, roundTripped.FontSize);
        Assert.Equal(original.Controls.Count, roundTripped.Controls.Count);

        for (int i = 0; i < original.Controls.Count; i++)
        {
            Assert.Equal(original.Controls[i].Id, roundTripped.Controls[i].Id);
            Assert.Equal(original.Controls[i].Style, roundTripped.Controls[i].Style);
            Assert.Equal(original.Controls[i].ClassOrdinal, roundTripped.Controls[i].ClassOrdinal);
            Assert.Equal(original.Controls[i].Text, roundTripped.Controls[i].Text);
        }
    }
}
