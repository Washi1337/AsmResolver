using AsmResolver.Symbols.Pdb.Types;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Types;

public class SimpleTypeTest
{
    [Theory]
    [InlineData(0x00_75, SimpleTypeKind.UInt32, SimpleTypeMode.Direct)]
    [InlineData(0x04_03, SimpleTypeKind.Void, SimpleTypeMode.NearPointer32)]
    public void TypeIndexParsing(uint typeIndex, SimpleTypeKind kind, SimpleTypeMode mode)
    {
        var type = new SimpleType(typeIndex);
        Assert.Equal(kind, type.Kind);
        Assert.Equal(mode, type.Mode);
    }
}
