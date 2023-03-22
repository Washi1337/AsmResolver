using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class FunctionListSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly FunctionListSymbol _symbol;

    public FunctionListSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("chandler4gs.obj"))
            .Symbols.OfType<ProcedureSymbol>().First()
            .Symbols.OfType<FunctionListSymbol>()
            .First();
    }

    [Fact]
    public void Entries()
    {
        Assert.Equal(new[]
        {
            ("_filter_x86_sse2_floating_point_exception", 0),
            ("_except_handler4_common", 0)
        }, _symbol.Entries.Select(x => (x.Function!.Name!.Value, x.Count)));
    }
}
