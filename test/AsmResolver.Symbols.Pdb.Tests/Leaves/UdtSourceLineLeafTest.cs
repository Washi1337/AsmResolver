using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class UdtSourceLineLeafTest
{
    [Fact]
    public void LeafKind()
    {
        var type = new SimpleTypeRecord(SimpleTypeKind.Int32);
        var srcFile = new SimpleTypeRecord(SimpleTypeKind.Void);
        var leaf = new UdtSourceLineLeaf(type, srcFile, 42);
        Assert.Equal(CodeViewLeafKind.UdtSrcLine, leaf.LeafKind);
    }

    [Fact]
    public void UdtType()
    {
        var type = new SimpleTypeRecord(SimpleTypeKind.Int32);
        var srcFile = new SimpleTypeRecord(SimpleTypeKind.Void);
        var leaf = new UdtSourceLineLeaf(type, srcFile, 42);
        Assert.Equal(SimpleTypeKind.Int32, Assert.IsAssignableFrom<SimpleTypeRecord>(leaf.UdtType).Kind);
    }

    [Fact]
    public void SourceFile()
    {
        var type = new SimpleTypeRecord(SimpleTypeKind.Int32);
        var srcFile = new SimpleTypeRecord(SimpleTypeKind.Void);
        var leaf = new UdtSourceLineLeaf(type, srcFile, 42);
        Assert.Equal(SimpleTypeKind.Void, Assert.IsAssignableFrom<SimpleTypeRecord>(leaf.SourceFile).Kind);
    }

    [Fact]
    public void Line()
    {
        var type = new SimpleTypeRecord(SimpleTypeKind.Int32);
        var srcFile = new SimpleTypeRecord(SimpleTypeKind.Void);
        var leaf = new UdtSourceLineLeaf(type, srcFile, 100);
        Assert.Equal(100u, leaf.Line);
    }

    [Fact]
    public void ToStringContainsLine()
    {
        var type = new SimpleTypeRecord(SimpleTypeKind.Int32);
        var srcFile = new SimpleTypeRecord(SimpleTypeKind.Void);
        var leaf = new UdtSourceLineLeaf(type, srcFile, 42);
        Assert.Equal("Int32 @ line 42", leaf.ToString());
    }
}
