using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class BasePointerRelativeSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public BasePointerRelativeSymbolTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Name()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("gs_report.obj"))
            .Symbols.OfType<BasePointerRelativeSymbol>()
            .First();

        Assert.Equal("exception_pointers", symbol.Name);
    }

    [Fact]
    public void Offset()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("gs_report.obj"))
            .Symbols.OfType<BasePointerRelativeSymbol>()
            .First();

        Assert.Equal(8, symbol.Offset);
    }

    [Fact]
    public void Type()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("gs_report.obj"))
            .Symbols.OfType<BasePointerRelativeSymbol>()
            .First();

        Assert.IsAssignableFrom<PointerTypeRecord>(symbol.VariableType);
    }

}
