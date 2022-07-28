using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

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

        Assert.Equal((CodeViewFieldAttributes.Public, "DISPLAYCONFIG_SCANLINE_ORDERING_UNSPECIFIED", 0u),
            enumerates[0]);
        Assert.Equal((CodeViewFieldAttributes.Public, "DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE", 1u),
            enumerates[1]);
        Assert.Equal((CodeViewFieldAttributes.Public, "DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED", 2u), enumerates[2]);
        Assert.Equal((CodeViewFieldAttributes.Public, "DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_UPPERFIELDFIRST", 2u),
            enumerates[3]);
        Assert.Equal((CodeViewFieldAttributes.Public, "DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_LOWERFIELDFIRST", 3u),
            enumerates[4]);
        Assert.Equal((CodeViewFieldAttributes.Public, "DISPLAYCONFIG_SCANLINE_ORDERING_FORCE_UINT32", '\xff'),
            enumerates[5]);
    }
}
