using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class FrameProcedureSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly FrameProcedureSymbol _symbol;

    public FrameProcedureSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("cpu_disp.obj"))
            .Symbols.OfType<ProcedureSymbol>().First()
            .Symbols.OfType<FrameProcedureSymbol>()
            .First();
    }

    [Fact]
    public void BasicProperties()
    {
        Assert.Equal(0x24u, _symbol.FrameSize);
        Assert.Equal(0u, _symbol.PaddingSize);
        Assert.Equal(0, _symbol.PaddingOffset);
        Assert.Equal(0xCu, _symbol.CalleeSavesSize);
        Assert.Equal(0, _symbol.ExceptionHandlerOffset);
        Assert.Equal(0, _symbol.ExceptionHandlerSection);
        Assert.False((_symbol.Attributes & FrameProcedureAttributes.ValidCounts) != 0);
        Assert.True((_symbol.Attributes & FrameProcedureAttributes.GuardCF) != 0);
    }
}
