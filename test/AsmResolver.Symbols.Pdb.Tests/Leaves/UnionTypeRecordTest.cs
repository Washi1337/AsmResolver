using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class UnionTypeRecordTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public UnionTypeRecordTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ReadSize()
    {
        var type = (UnionTypeRecord) _fixture.SimplePdb.GetLeafRecord(0x1855);
        Assert.Equal(4ul, type.Size);
    }

    [Fact]
    public void ReadName()
    {
        var type = (UnionTypeRecord) _fixture.SimplePdb.GetLeafRecord(0x1855);
        Assert.Equal("_LDT_ENTRY::<unnamed-type-HighWord>", type.Name);
    }

    [Fact]
    public void ReadUniqueName()
    {
        var type = (UnionTypeRecord) _fixture.SimplePdb.GetLeafRecord(0x1855);
        Assert.Equal(".?AT<unnamed-type-HighWord>@_LDT_ENTRY@@", type.UniqueName);
    }

    [Fact]
    public void ReadFieldList()
    {
        var type = (UnionTypeRecord) _fixture.SimplePdb.GetLeafRecord(0x1855);
        var fields = type.Fields!;
        Assert.NotNull(fields);
        Assert.IsAssignableFrom<NestedTypeField>(fields.Entries[0]);
        Assert.IsAssignableFrom<InstanceDataField>(fields.Entries[1]);
        Assert.IsAssignableFrom<NestedTypeField>(fields.Entries[2]);
        Assert.IsAssignableFrom<InstanceDataField>(fields.Entries[3]);
    }
}
