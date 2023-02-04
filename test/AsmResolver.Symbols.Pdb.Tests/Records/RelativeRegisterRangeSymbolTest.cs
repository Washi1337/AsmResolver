using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class RelativeRegisterRangeSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public RelativeRegisterRangeSymbolTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void BasicProperties()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("dllmain.obj"))
            .Symbols.OfType<RelativeRegisterRangeSymbol>()
            .First();

        Assert.Equal(4, symbol.Offset);
        Assert.Equal(0u, symbol.Range.Start);
        Assert.Equal(1u, symbol.Range.SectionStart);
        Assert.Equal(1u, symbol.Range.Length);
    }

    [Fact]
    public void NoGaps()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("dllmain.obj"))
            .Symbols.OfType<RelativeRegisterRangeSymbol>()
            .First();

        Assert.Empty(symbol.Gaps);
    }
}
