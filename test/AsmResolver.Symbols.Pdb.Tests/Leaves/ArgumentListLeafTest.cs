using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class ArgumentListLeafTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public ArgumentListLeafTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ReadMultipleTypes()
    {
        var list = (ArgumentListLeaf) _fixture.SimplePdb.GetLeafRecord(0x2391);
        Assert.IsAssignableFrom<PointerTypeRecord>(list.Types[0]);
        Assert.IsAssignableFrom<SimpleTypeRecord>(list.Types[1]);
    }
}
