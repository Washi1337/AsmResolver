using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class MemberFunctionLeafTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public MemberFunctionLeafTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ReadReturnType()
    {
        var function = (MemberFunctionLeaf) _fixture.SimplePdb.GetLeafRecord(0x2392);
        Assert.Equal(SimpleTypeKind.Void, Assert.IsAssignableFrom<SimpleTypeRecord>(function.ReturnType).Kind);
    }

    [Fact]
    public void ReadDeclaringType()
    {
        var function = (MemberFunctionLeaf) _fixture.SimplePdb.GetLeafRecord(0x2392);
        Assert.Equal("std::bad_cast", Assert.IsAssignableFrom<ClassTypeRecord>(function.DeclaringType).Name);
    }

    [Fact]
    public void ReadNonNullThisType()
    {
        var function = (MemberFunctionLeaf) _fixture.SimplePdb.GetLeafRecord(0x2392);
        Assert.IsAssignableFrom<PointerTypeRecord>(function.ThisType);
    }

    [Fact]
    public void ReadArgumentList()
    {
        var function = (MemberFunctionLeaf) _fixture.SimplePdb.GetLeafRecord(0x2392);
        Assert.NotNull(function.Arguments);
        Assert.IsAssignableFrom<PointerTypeRecord>(function.Arguments!.Types[0]);
        Assert.IsAssignableFrom<SimpleTypeRecord>(function.Arguments.Types[1]);
    }
}
