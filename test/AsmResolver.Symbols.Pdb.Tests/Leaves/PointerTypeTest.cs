using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class PointerTypeTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public PointerTypeTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void CreateNewType()
    {
        var type = new PointerType(new SimpleType(SimpleTypeKind.Character8), PointerAttributes.Const, 4);
        Assert.True(type.IsConst);
        Assert.Equal(4, type.Size);
    }

    [Fact]
    public void UpdateKind()
    {
        var type = new PointerType(new SimpleType(SimpleTypeKind.Character8), PointerAttributes.Const, 4);
        type.Kind = PointerAttributes.Near32;
        Assert.Equal(PointerAttributes.Near32, type.Kind);
    }

    [Fact]
    public void UpdateMode()
    {
        var type = new PointerType(new SimpleType(SimpleTypeKind.Character8), PointerAttributes.Const, 4);
        type.Mode = PointerAttributes.LValueReference;
        Assert.Equal(PointerAttributes.LValueReference, type.Mode);
    }

    [Fact]
    public void UpdateSize()
    {
        var type = new PointerType(new SimpleType(SimpleTypeKind.Character8), PointerAttributes.Const, 4);
        type.Size = 8;
        Assert.Equal(8, type.Size);
    }

    [Fact]
    public void ReadAttributes()
    {
        var type = (PointerType) _fixture.SimplePdb.GetLeafRecord(0x1012);
        Assert.True(type.IsNear32);
    }

    [Fact]
    public void ReadBaseType()
    {
        var type = (PointerType) _fixture.SimplePdb.GetLeafRecord(0x1012);
        Assert.Equal(CodeViewLeafKind.Modifier, type.BaseType.LeafKind);
    }
}
