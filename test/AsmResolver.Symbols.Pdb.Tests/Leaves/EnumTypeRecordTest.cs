using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class EnumTypeRecordTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public EnumTypeRecordTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void FieldList()
    {
        var leaf = _fixture.SimplePdb.GetLeafRecord(0x1009);
        var fields = Assert.IsAssignableFrom<EnumTypeRecord>(leaf).Fields!.Entries.Cast<EnumerateField>().ToArray();
        var names = new[]
        {
            "DISPLAYCONFIG_SCANLINE_ORDERING_UNSPECIFIED",
            "DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE",
            "DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED",
            "DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_UPPERFIELDFIRST",
            "DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_LOWERFIELDFIRST",
            "DISPLAYCONFIG_SCANLINE_ORDERING_FORCE_UINT32",
        };
        Assert.Equal(names[0], fields[0].Name);
        Assert.Equal(names[1], fields[1].Name);
        Assert.Equal(names[2], fields[2].Name);
        Assert.Equal(names[3], fields[3].Name);
        Assert.Equal(names[4], fields[4].Name);
        Assert.Equal(names[5], fields[5].Name);
    }
}
