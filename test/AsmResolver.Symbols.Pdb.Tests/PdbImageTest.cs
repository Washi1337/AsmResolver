using AsmResolver.Symbols.Pdb.Types;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests;

public class PdbImageTest
{
    [Theory]
    [InlineData(0x00_75, SimpleTypeKind.UInt32, SimpleTypeMode.Direct)]
    [InlineData(0x04_03, SimpleTypeKind.Void, SimpleTypeMode.NearPointer32)]
    public void SimpleTypeLookup(uint typeIndex, SimpleTypeKind kind, SimpleTypeMode mode)
    {
        var image = PdbImage.FromBytes(Properties.Resources.SimpleDllPdb);

        var type = Assert.IsAssignableFrom<SimpleType>(image.GetTypeRecord(typeIndex));
        Assert.Equal(kind, type.Kind);
        Assert.Equal(mode, type.Mode);
    }

    [Fact]
    public void SimpleTypeLookupTwiceShouldCache()
    {
        var image = PdbImage.FromBytes(Properties.Resources.SimpleDllPdb);

        var type = image.GetTypeRecord(0x00_75);
        var type2 = image.GetTypeRecord(0x00_75);

        Assert.Same(type, type2);
    }
}
