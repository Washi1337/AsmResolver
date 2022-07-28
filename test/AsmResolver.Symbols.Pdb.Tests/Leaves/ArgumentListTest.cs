using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class ArgumentListTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public ArgumentListTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ReadMultipleTypes()
    {
        var list = (ArgumentList) _fixture.SimplePdb.GetLeafRecord(0x2391);
        Assert.IsAssignableFrom<PointerType>(list.Types[0]);
        Assert.IsAssignableFrom<SimpleType>(list.Types[1]);
    }
}
