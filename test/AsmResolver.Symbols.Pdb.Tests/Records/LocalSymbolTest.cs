using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class LocalSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly LocalSymbol _symbol;

    public LocalSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("dllmain.obj"))
            .Symbols.OfType<ProcedureSymbol>().First()
            .Symbols.OfType<LocalSymbol>()
            .First();
    }

    [Fact]
    public void Name()
    {
        Assert.Equal("hModule", _symbol.Name);
    }

    [Fact]
    public void Type()
    {
        Assert.IsAssignableFrom<PointerTypeRecord>(_symbol.VariableType);
    }
}
