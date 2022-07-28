using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class FunctionTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public FunctionTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ReadReturnType()
    {
        var function = (MemberFunction) _fixture.SimplePdb.GetLeafRecord(0x2392);
        Assert.Equal(SimpleTypeKind.Void, Assert.IsAssignableFrom<SimpleType>(function.ReturnType).Kind);
    }

    [Fact]
    public void ReadDeclaringType()
    {
        var function = (MemberFunction) _fixture.SimplePdb.GetLeafRecord(0x2392);
        Assert.Equal("std::bad_cast", Assert.IsAssignableFrom<ClassType>(function.DeclaringType).Name);
    }

    [Fact]
    public void ReadNonNullThisType()
    {
        var function = (MemberFunction) _fixture.SimplePdb.GetLeafRecord(0x2392);
        Assert.IsAssignableFrom<PointerType>(function.ThisType);
    }

    [Fact]
    public void ReadArgumentList()
    {
        var function = (MemberFunction) _fixture.SimplePdb.GetLeafRecord(0x2392);
        Assert.NotNull(function.Arguments);
        Assert.IsAssignableFrom<PointerType>(function.Arguments!.Types[0]);
        Assert.IsAssignableFrom<SimpleType>(function.Arguments.Types[1]);
    }
}
