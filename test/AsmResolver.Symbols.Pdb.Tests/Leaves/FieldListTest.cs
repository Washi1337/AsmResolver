using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;
using static AsmResolver.Symbols.Pdb.Leaves.CodeViewFieldAttributes;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class FieldListTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public FieldListTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ReadEnumerateList()
    {
        var list = (FieldList) _fixture.SimplePdb.GetLeafRecord(0x1008);
        var enumerates = list.Entries
            .Cast<EnumerateField>()
            .Select(f => (f.Attributes, f.Name.Value, f.Value))
            .ToArray();

        Assert.Equal((Public, "DISPLAYCONFIG_SCANLINE_ORDERING_UNSPECIFIED", 0u), enumerates[0]);
        Assert.Equal((Public, "DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE", 1u), enumerates[1]);
        Assert.Equal((Public, "DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED", 2u), enumerates[2]);
        Assert.Equal((Public, "DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_UPPERFIELDFIRST", 2u), enumerates[3]);
        Assert.Equal((Public, "DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_LOWERFIELDFIRST", 3u), enumerates[4]);
        Assert.Equal((Public, "DISPLAYCONFIG_SCANLINE_ORDERING_FORCE_UINT32", '\xff'), enumerates[5]);
    }

    [Fact]
    public void ReadInstanceDataMemberList()
    {
        var list = (FieldList) _fixture.SimplePdb.GetLeafRecord(0x1017);
        var enumerates = list.Entries
            .Cast<InstanceDataMember>()
            .Select(f => (f.Attributes, f.Name.Value, f.Offset))
            .ToArray();

        Assert.Equal((Public, "cbSize", 0ul), enumerates[0]);
        Assert.Equal((Public, "fMask", 4ul), enumerates[1]);
        Assert.Equal((Public, "fType", 8ul), enumerates[2]);
        Assert.Equal((Public, "fState", 12ul), enumerates[3]);
        Assert.Equal((Public, "wID", 16ul), enumerates[4]);
        Assert.Equal((Public, "hSubMenu", 20ul), enumerates[5]);
        Assert.Equal((Public, "hbmpChecked", 24ul), enumerates[6]);
        Assert.Equal((Public, "hbmpUnchecked", 28ul), enumerates[7]);
        Assert.Equal((Public, "dwItemData", 32ul), enumerates[8]);
        Assert.Equal((Public, "dwTypeData", 36ul), enumerates[9]);
        Assert.Equal((Public, "cch", 40ul), enumerates[10]);
        Assert.Equal((Public, "hbmpItem", 44ul), enumerates[11]);
    }
}
