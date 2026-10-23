using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class LabelTypeRecordTest
{
    [Fact]
    public void CreateNearLabel()
    {
        var label = new LabelTypeRecord(LabelAddressingMode.Near);
        Assert.Equal(CodeViewLeafKind.Label, label.LeafKind);
        Assert.Equal(LabelAddressingMode.Near, label.Mode);
    }

    [Fact]
    public void CreateFarLabel()
    {
        var label = new LabelTypeRecord(LabelAddressingMode.Far);
        Assert.Equal(CodeViewLeafKind.Label, label.LeafKind);
        Assert.Equal(LabelAddressingMode.Far, label.Mode);
    }

    [Fact]
    public void SetMode()
    {
        var label = new LabelTypeRecord(LabelAddressingMode.Near);
        label.Mode = LabelAddressingMode.Far;
        Assert.Equal(LabelAddressingMode.Far, label.Mode);
    }

    [Fact]
    public void ToStringContainsMode()
    {
        var label = new LabelTypeRecord(LabelAddressingMode.Near);
        Assert.Equal("Label (Near)", label.ToString());
    }
}
