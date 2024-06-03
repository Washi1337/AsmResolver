using System.Linq;
using AsmResolver.PE.File;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class SectionSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly SectionSymbol _symbol;

    public SectionSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name == "* Linker *")
            .Symbols.OfType<SectionSymbol>().First();
    }

    [Fact]
    public void BasicProperties()
    {
        Assert.Equal(1, _symbol.SectionNumber);
        Assert.Equal(0x1000u, _symbol.Rva);
        Assert.Equal(0xCE8u, _symbol.Size);
        Assert.Equal(0x1000u, _symbol.Alignment);
        Assert.Equal(
            SectionFlags.MemoryRead | SectionFlags.MemoryExecute | SectionFlags.ContentCode,
            _symbol.Attributes);
    }

    [Fact]
    public void Name()
    {
        Assert.Equal(".text", _symbol.Name);
    }
}
