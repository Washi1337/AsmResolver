using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class FunctionListSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public FunctionListSymbolTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Entries()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("chandler4gs.obj"))
            .Symbols.OfType<FunctionListSymbol>()
            .First();

        Assert.Equal(new[]
        {
            ("_filter_x86_sse2_floating_point_exception", 0),
            ("_except_handler4_common", 0)
        }, symbol.Entries.Select(x => (x.Function!.Name!.Value, x.Count)));
    }
}
