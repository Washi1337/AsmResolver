using AsmResolver.Symbols.Pdb.Leaves;
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
        var type = Assert.IsAssignableFrom<SimpleTypeRecord>(_fixture.SimplePdb.GetLeafRecord(typeIndex));
        Assert.Equal(kind, type.Kind);
        Assert.Equal(mode, type.Mode);
    }

    [Fact]
    public void SimpleTypeLookupTwiceShouldCache()
    {
        var image = _fixture.SimplePdb;

        var type = image.GetLeafRecord(0x00_75);
        var type2 = image.GetLeafRecord(0x00_75);

        Assert.Same(type, type2);
    }
}
