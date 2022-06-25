using AsmResolver.Symbols.Pdb.Metadata;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Metadata;

public class PdbHashTest
{
    [Theory]
    [InlineData("/UDTSRCLINEUNDONE", 0x23296bb2)]
    [InlineData("/src/headerblock", 0x2b237ecd)]
    [InlineData("/LinkInfo", 0x282209ed)]
    [InlineData("/TMCache", 0x2621d5e9)]
    [InlineData("/names", 0x6d6cfc21)]
    public void HashV1(string value, uint expected)
    {
        Assert.Equal(expected, PdbHash.ComputeV1(value));
    }
}
