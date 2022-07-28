using System;
using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class ClassTypeTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public ClassTypeTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(CodeViewLeafKind.Class)]
    [InlineData(CodeViewLeafKind.Structure)]
    [InlineData(CodeViewLeafKind.Interface)]
    public void CreateNewValidType(CodeViewLeafKind kind)
    {
        var type = new ClassType(kind, "MyType", "MyUniqueType", 4, StructureAttributes.FwdRef, null);
        Assert.Equal(kind, type.LeafKind);
        Assert.Equal("MyType", type.Name);
        Assert.Equal("MyUniqueType", type.UniqueName);
        Assert.Equal(4u, type.Size);
        Assert.Equal(StructureAttributes.FwdRef, type.StructureAttributes);
        Assert.Null(type.BaseType);
    }

    [Fact]
    public void CreateNonValidType()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ClassType(
            CodeViewLeafKind.Char,
            "Invalid",
            "Invalid",
            4,
            StructureAttributes.FwdRef,
            null));
    }

    [Fact]
    public void ReadFieldList()
    {
        var type = (ClassType) _fixture.SimplePdb.GetLeafRecord(0x101b);
        Assert.NotNull(type.Fields);
    }

    [Fact]
    public void ReadVTableShape()
    {
        var type = (ClassType) _fixture.SimplePdb.GetLeafRecord(0x239f);
        Assert.NotNull(type.VTableShape);
        Assert.Equal(new[]
        {
            VTableShapeEntry.Near32,
            VTableShapeEntry.Near32,
        }, type.VTableShape.Entries);
    }
}
