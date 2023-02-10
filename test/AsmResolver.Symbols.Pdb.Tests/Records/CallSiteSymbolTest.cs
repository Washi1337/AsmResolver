using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class CallSiteSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public CallSiteSymbolTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Offset()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("gs_report.obj"))
            .Symbols.OfType<CallSiteSymbol>()
            .First();

        Assert.Equal(0x0000037E, symbol.Offset);
    }

    [Fact]
    public void FunctionType()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("gs_report.obj"))
            .Symbols.OfType<CallSiteSymbol>()
            .First();

        Assert.IsAssignableFrom<PointerTypeRecord>(symbol.FunctionType);
    }
}
