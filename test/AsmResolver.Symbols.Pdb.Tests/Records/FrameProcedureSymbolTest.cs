using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class FrameProcedureSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public FrameProcedureSymbolTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void BasicProperties()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("cpu_disp.obj"))
            .Symbols.OfType<FrameProcedureSymbol>()
            .First();

        Assert.Equal(0x24u, symbol.FrameSize);
        Assert.Equal(0u, symbol.PaddingSize);
        Assert.Equal(0, symbol.PaddingOffset);
        Assert.Equal(0xCu, symbol.CalleeSavesSize);
        Assert.Equal(0, symbol.ExceptionHandlerOffset);
        Assert.Equal(0, symbol.ExceptionHandlerSection);
        Assert.False((symbol.Attributes & FrameProcedureAttributes.ValidCounts) != 0);
        Assert.True((symbol.Attributes & FrameProcedureAttributes.GuardCF) != 0);
    }
}
