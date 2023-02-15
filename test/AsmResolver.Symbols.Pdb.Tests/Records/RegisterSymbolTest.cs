using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class RegisterSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly RegisterSymbol _symbol;

    public RegisterSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains(@"msvcrt.nativeproj_110336922\objr\x86\secchk.obj"))
            .Symbols.OfType<ProcedureSymbol>().First()
            .Symbols.OfType<RegisterSymbol>().First();
    }

    [Fact]
    public void Name()
    {
        Assert.Equal("cookie", _symbol.Name);
    }

    [Fact]
    public void Type()
    {
        Assert.Equal(SimpleTypeKind.UInt32, Assert.IsAssignableFrom<SimpleTypeRecord>(_symbol.VariableType).Kind);
    }
}
