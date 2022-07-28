using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class ModifierTypeTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public ModifierTypeTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void CreateNewType()
    {
        var type = new ModifierType(new SimpleType(SimpleTypeKind.Character8), ModifierAttributes.Const);
        Assert.True(type.IsConst);
    }

    [Fact]
    public void ReadAttributes()
    {
        var type = (ModifierType) _fixture.SimplePdb.GetLeafRecord(0x1011);
        Assert.True(type.IsConst);
    }

    [Fact]
    public void ReadBaseType()
    {
        var type = (ModifierType) _fixture.SimplePdb.GetLeafRecord(0x1011);
        Assert.Equal(CodeViewLeafKind.Structure, type.BaseType.LeafKind);
    }
}
