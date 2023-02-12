using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class FrameCookieSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly FrameCookieSymbol _symbol;

    public FrameCookieSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains(@"msvcrt.nativeproj_110336922\objr\x86\dll_dllmain.obj"))
            .Symbols.OfType<ProcedureSymbol>().ElementAt(1)
            .Symbols.OfType<FrameCookieSymbol>()
            .First();
    }

    [Fact]
    public void BasicProperties()
    {
        Assert.Equal(-48, _symbol.FrameOffset);
        Assert.Equal(FrameCookieType.Copy, _symbol.CookieType);
        Assert.Equal(0, _symbol.Attributes);
    }
}
