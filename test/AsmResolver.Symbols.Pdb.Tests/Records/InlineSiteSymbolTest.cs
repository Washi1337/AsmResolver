using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class InlineSiteSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly InlineSiteSymbol _symbol;

    public InlineSiteSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains(@"msvcrt.nativeproj_110336922\objr\x86\dll_dllmain.obj"))
            .Symbols.OfType<ProcedureSymbol>().ElementAt(3)
            .Symbols.OfType<InlineSiteSymbol>().First();
    }

    [Fact]
    public void Inlinee()
    {
        Assert.NotNull(_symbol.Inlinee);
        Assert.Equal("dllmain_crt_dispatch", _symbol.Inlinee.Name);
    }

    [Fact]
    public void BinaryAnnotations()
    {
        Assert.Equal(new[]
        {
            BinaryAnnotationOpCode.ChangeCodeLengthAndCodeOffset
        }, _symbol.Annotations.Select(x => x.OpCode));
    }
}
