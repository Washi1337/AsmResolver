using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class LabelSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly LabelSymbol _symbol;

    public LabelSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains(@"msvcrt.nativeproj_110336922\objr\x86\secchk.obj"))
            .Symbols.OfType<ProcedureSymbol>().First()
            .Symbols.OfType<LabelSymbol>().First();
    }

    [Fact]
    public void BasicProperties()
    {
        Assert.Equal(1, _symbol.SegmentIndex);
        Assert.Equal(0x11u, _symbol.Offset);
    }

    [Fact]
    public void Name()
    {
        Assert.Equal("failure", _symbol.Name);
    }
}
