using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class VTableShapeTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public VTableShapeTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(0x2416, new[] { VTableShapeEntry.Near })]
    [InlineData(0x239e, new[] { VTableShapeEntry.Near32,VTableShapeEntry.Near32 })]
    public void ReadEntries(uint typeIndex, VTableShapeEntry[] expectedEntries)
    {
        var shape = (VTableShape) _fixture.SimplePdb.GetLeafRecord(typeIndex);
        Assert.Equal(expectedEntries, shape.Entries);
    }
}
