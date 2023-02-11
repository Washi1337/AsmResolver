using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class CallSiteSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly CallSiteSymbol _symbol;

    public CallSiteSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("gs_report.obj"))
            .Symbols.OfType<ProcedureSymbol>().First()
            .Symbols.OfType<CallSiteSymbol>()
            .First();
    }

    [Fact]
    public void Offset()
    {
        Assert.Equal(0x0000037E, _symbol.Offset);
    }

    [Fact]
    public void FunctionType()
    {
        Assert.IsAssignableFrom<PointerTypeRecord>(_symbol.FunctionType);
    }
}
