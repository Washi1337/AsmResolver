using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class RegisterRangeSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly RegisterRangeSymbol _symbol;

    public RegisterRangeSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("gs_support.obj"))
            .Symbols.OfType<ProcedureSymbol>().First()
            .Symbols.OfType<RegisterRangeSymbol>().First();
    }

    [Fact]
    public void BasicProperties()
    {
        Assert.Equal(0x0001, _symbol.Range.SectionStart);
        Assert.Equal(0x000004E1u, _symbol.Range.Start);
        Assert.Equal(0x000004E8 - 0x000004E1u, _symbol.Range.Length);
    }

    [Fact]
    public void EmptyGaps()
    {
        Assert.Empty(_symbol.Gaps);
    }
}
