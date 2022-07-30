using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class UserDefinedTypeTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public UserDefinedTypeTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Name()
    {
        var udt = _fixture.SimplePdb.Symbols.OfType<UserDefinedTypeSymbol>().First();
        Assert.Equal("UINT", udt.Name);
    }

    [Fact]
    public void Type()
    {
        var udt = _fixture.SimplePdb.Symbols.OfType<UserDefinedTypeSymbol>().First();
        var type = Assert.IsAssignableFrom<SimpleTypeRecord>(udt.Type);
        Assert.Equal(SimpleTypeKind.UInt32, type.Kind);
    }

    [Fact]
    public void Type2()
    {
        var udt = _fixture.SimplePdb.Symbols.OfType<UserDefinedTypeSymbol>().ElementAt(1);
        Assert.Equal(CodeViewLeafKind.Pointer, udt.Type.LeafKind);
    }
}
