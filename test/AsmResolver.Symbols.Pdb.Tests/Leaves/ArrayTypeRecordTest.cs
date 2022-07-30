using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class ArrayTypeRecordTest: IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public ArrayTypeRecordTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ReadElementType()
    {
        var type = (ArrayTypeRecord) _fixture.SimplePdb.GetLeafRecord(0x1905);
        Assert.Equal(SimpleTypeKind.Void, Assert.IsAssignableFrom<SimpleTypeRecord>(type.ElementType).Kind);
    }

    [Fact]
    public void ReadIndexType()
    {
        var type = (ArrayTypeRecord) _fixture.SimplePdb.GetLeafRecord(0x1905);
        Assert.Equal(SimpleTypeKind.UInt32Long, Assert.IsAssignableFrom<SimpleTypeRecord>(type.IndexType).Kind);
    }

    [Fact]
    public void ReadLength()
    {
        var type = (ArrayTypeRecord) _fixture.SimplePdb.GetLeafRecord(0x1905);
        Assert.Equal(4u, type.Length);
    }

    [Fact]
    public void ReadEmptyName()
    {
        var type = (ArrayTypeRecord) _fixture.SimplePdb.GetLeafRecord(0x1905);
        Assert.True(Utf8String.IsNullOrEmpty(type.Name));
    }
}
