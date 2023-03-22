using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class RegisterRelativeSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly RegisterRelativeSymbol _symbol;

    public RegisterRelativeSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("dllmain.obj"))
            .Symbols.OfType<ProcedureSymbol>().First()
            .Symbols.OfType<RegisterRelativeSymbol>()
            .First();
    }

    [Fact]
    public void Offset()
    {
        Assert.Equal(8, _symbol.Offset);
    }

    [Fact]
    public void Type()
    {
        Assert.IsAssignableFrom<PointerTypeRecord>(_symbol.VariableType);
    }

    [Fact]
    public void Name()
    {
        Assert.Equal("hModule", _symbol.Name);
    }

}
