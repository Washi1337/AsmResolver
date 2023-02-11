using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class ProcedureSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public ProcedureSymbolTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Global()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains(@"SimpleDll\Release\dllmain.obj"))
            .Symbols.OfType<ProcedureSymbol>()
            .First();

        Assert.True(symbol.IsGlobal);
        Assert.Equal(CodeViewSymbolType.GProc32, symbol.CodeViewSymbolType);
    }

    [Fact]
    public void Local()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains(@"gs_support.obj"))
            .Symbols.OfType<ProcedureSymbol>()
            .First();

        Assert.True(symbol.IsLocal);
        Assert.Equal(CodeViewSymbolType.LProc32, symbol.CodeViewSymbolType);
    }

    [Fact]
    public void Name()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains(@"SimpleDll\Release\dllmain.obj"))
            .Symbols.OfType<ProcedureSymbol>()
            .First();

        Assert.Equal("DllMain", symbol.Name);
    }

    [Fact]
    public void Size()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains(@"SimpleDll\Release\dllmain.obj"))
            .Symbols.OfType<ProcedureSymbol>()
            .First();

        Assert.Equal(8u, symbol.Size);
    }

    [Fact]
    public void DebugOffsets()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains(@"SimpleDll\Release\dllmain.obj"))
            .Symbols.OfType<ProcedureSymbol>()
            .First();

        Assert.Equal(0u, symbol.DebugStartOffset);
        Assert.Equal(5u, symbol.DebugEndOffset);
    }

    [Fact]
    public void Attributes()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains(@"SimpleDll\Release\dllmain.obj"))
            .Symbols.OfType<ProcedureSymbol>()
            .First();

        Assert.Equal(ProcedureAttributes.OptimizedDebugInfo, symbol.Attributes);
    }

    [Fact]
    public void ProcedureType()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains(@"SimpleDll\Release\dllmain.obj"))
            .Symbols.OfType<ProcedureSymbol>()
            .First();

        Assert.NotNull(symbol.ProcedureType);
    }
}
