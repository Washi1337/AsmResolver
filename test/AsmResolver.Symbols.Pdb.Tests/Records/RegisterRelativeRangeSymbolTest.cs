using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class RegisterRelativeRangeSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly RegisterRelativeRangeSymbol _symbol;

    public RegisterRelativeRangeSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("dllmain.obj"))
            .Symbols.OfType<ProcedureSymbol>().First()
            .Symbols.OfType<RegisterRelativeRangeSymbol>()
            .First();
    }

    [Fact]
    public void BasicProperties()
    {
        Assert.Equal(4, _symbol.Offset);
        Assert.Equal(0u, _symbol.Range.Start);
        Assert.Equal(1u, _symbol.Range.SectionStart);
        Assert.Equal(1u, _symbol.Range.Length);
    }

    [Fact]
    public void NoGaps()
    {
        Assert.Empty(_symbol.Gaps);
    }
}
