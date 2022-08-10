using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class BitFieldTypeRecordTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public BitFieldTypeRecordTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ReadBaseType()
    {
        var type = (BitFieldTypeRecord) _fixture.MyTestApplication.GetLeafRecord(0x1060);
        Assert.Equal(SimpleTypeKind.UInt32Long, Assert.IsAssignableFrom<SimpleTypeRecord>(type.Type).Kind);
    }

    [Fact]
    public void ReadPosition()
    {
        var type = (BitFieldTypeRecord) _fixture.MyTestApplication.GetLeafRecord(0x1060);
        Assert.Equal(2, type.Position);
    }

    [Fact]
    public void ReadLength()
    {
        var type = (BitFieldTypeRecord) _fixture.MyTestApplication.GetLeafRecord(0x1060);
        Assert.Equal(30, type.Length);
    }
}
