using System.Linq;
using AsmResolver.PE.File;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class CoffGroupSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly CoffGroupSymbol _symbol;

    public CoffGroupSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name == "* Linker *")
            .Symbols.OfType<CoffGroupSymbol>()
            .First();
    }

    [Fact]
    public void BasicProperties()
    {
        Assert.Equal(1, _symbol.SegmentIndex);
        Assert.Equal(0u, _symbol.Offset);
        Assert.Equal(0xCE8u, _symbol.Size);
        Assert.Equal(
            SectionFlags.ContentCode | SectionFlags.MemoryRead | SectionFlags.MemoryExecute,
            _symbol.Characteristics);
    }

    [Fact]
    public void Name()
    {
        Assert.Equal(".text$mn", _symbol.Name);
    }
}
