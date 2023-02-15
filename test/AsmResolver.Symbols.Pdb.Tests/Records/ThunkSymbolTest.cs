using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class ThunkSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly ThunkSymbol _symbol;

    public ThunkSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name == "Import:VCRUNTIME140.dll")
            .Symbols.OfType<ThunkSymbol>()
            .First();
    }

    [Fact]
    public void BasicProperties()
    {
        Assert.Equal(1, _symbol.SegmentIndex);
        Assert.Equal(0xC28u, _symbol.Offset);
        Assert.Equal(6u, _symbol.Size);
    }

    [Fact]
    public void Name()
    {
        Assert.Equal("__std_type_info_destroy_list", _symbol.Name);
    }

    [Fact]
    public void EmptyChildren()
    {
        Assert.Empty(_symbol.Symbols);
    }
}
