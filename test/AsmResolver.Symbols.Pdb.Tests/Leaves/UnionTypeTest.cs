using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class UnionTypeTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public UnionTypeTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ReadSize()
    {
        var type = (UnionType) _fixture.SimplePdb.GetLeafRecord(0x1855);
        Assert.Equal(4ul, type.Size);
    }

    [Fact]
    public void ReadName()
    {
        var type = (UnionType) _fixture.SimplePdb.GetLeafRecord(0x1855);
        Assert.Equal("_LDT_ENTRY::<unnamed-type-HighWord>", type.Name);
    }

    [Fact]
    public void ReadUniqueName()
    {
        var type = (UnionType) _fixture.SimplePdb.GetLeafRecord(0x1855);
        Assert.Equal(".?AT<unnamed-type-HighWord>@_LDT_ENTRY@@", type.UniqueName);
    }

    [Fact]
    public void ReadFieldList()
    {
        var type = (UnionType) _fixture.SimplePdb.GetLeafRecord(0x1855);
        var fields = type.Fields!;
        Assert.NotNull(fields);
        Assert.IsAssignableFrom<NestedType>(fields.Entries[0]);
        Assert.IsAssignableFrom<InstanceDataMember>(fields.Entries[1]);
        Assert.IsAssignableFrom<NestedType>(fields.Entries[2]);
        Assert.IsAssignableFrom<InstanceDataMember>(fields.Entries[3]);
    }
}
