using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class RegisterRelativeSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public RegisterRelativeSymbolTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Offset()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("dllmain.obj"))
            .Symbols.OfType<RegisterRelativeSymbol>()
            .First();

        Assert.Equal(8, symbol.Offset);
    }

    [Fact]
    public void Type()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("dllmain.obj"))
            .Symbols.OfType<RegisterRelativeSymbol>()
            .First();

        Assert.IsAssignableFrom<PointerTypeRecord>(symbol.VariableType);
    }

    [Fact]
    public void Name()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("dllmain.obj"))
            .Symbols.OfType<RegisterRelativeSymbol>()
            .First();

        Assert.Equal("hModule", symbol.Name);
    }

}
