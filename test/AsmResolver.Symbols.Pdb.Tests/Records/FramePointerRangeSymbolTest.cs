using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class FramePointerRangeSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;
    private readonly FramePointerRangeSymbol _symbol;

    public FramePointerRangeSymbolTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("dllmain.obj"))
            .Symbols.OfType<ProcedureSymbol>().First()
            .Symbols.OfType<FramePointerRangeSymbol>()
            .First();
    }

    [Fact]
    public void Offset()
    {
        Assert.Equal(8, _symbol.Offset);
    }

    [Fact]
    public void FullScope()
    {
        Assert.True(_symbol.IsFullScope);
        Assert.Equal(default, _symbol.Range);
        Assert.Empty(_symbol.Gaps);
    }

    [Fact]
    public void NonFullScope()
    {
        string path = @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\cpu_disp.obj";

        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name == path)
            .Symbols.OfType<ProcedureSymbol>().First()
            .Symbols.OfType<FramePointerRangeSymbol>()
            .First();

        Assert.False(symbol.IsFullScope);
        Assert.Equal(-12, symbol.Offset);
        Assert.Equal(0xa95u, symbol.Range.Start);
        Assert.Equal(0xc18u, symbol.Range.End);
        Assert.Empty(symbol.Gaps);
    }
}
