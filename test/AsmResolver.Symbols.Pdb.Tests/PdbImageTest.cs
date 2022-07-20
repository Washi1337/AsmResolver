using AsmResolver.Symbols.Pdb.Types;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests;

public class PdbImageTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public PdbImageTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(0x00_75, SimpleTypeKind.UInt32, SimpleTypeMode.Direct)]
    [InlineData(0x04_03, SimpleTypeKind.Void, SimpleTypeMode.NearPointer32)]
    public void SimpleTypeLookup(uint typeIndex, SimpleTypeKind kind, SimpleTypeMode mode)
    {
        var type = Assert.IsAssignableFrom<SimpleType>(_fixture.SimplePdb.GetTypeRecord(typeIndex));
        Assert.Equal(kind, type.Kind);
        Assert.Equal(mode, type.Mode);
    }

    [Fact]
    public void SimpleTypeLookupTwiceShouldCache()
    {
        var image = _fixture.SimplePdb;

        var type = image.GetTypeRecord(0x00_75);
        var type2 = image.GetTypeRecord(0x00_75);

        Assert.Same(type, type2);
    }
}
