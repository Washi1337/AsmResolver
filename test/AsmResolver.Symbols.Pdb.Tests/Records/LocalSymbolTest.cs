using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class LocalSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public LocalSymbolTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Name()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("dllmain.obj"))
            .Symbols.OfType<LocalSymbol>()
            .First();

        Assert.Equal("hModule", symbol.Name);
    }

    [Fact]
    public void Type()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("dllmain.obj"))
            .Symbols.OfType<LocalSymbol>()
            .First();

        Assert.IsAssignableFrom<PointerTypeRecord>(symbol.VariableType);
    }
}
