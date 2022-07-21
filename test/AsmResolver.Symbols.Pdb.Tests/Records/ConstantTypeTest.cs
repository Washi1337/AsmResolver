using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class ConstantTypeTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public ConstantTypeTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Name()
    {
        Assert.Equal("JOB_OBJECT_IO_RATE_CONTROL_STANDALONE_VOLUME", _fixture.SimplePdb.Symbols.OfType<ConstantSymbol>().First().Name);
    }

    [Fact]
    public void Type()
    {
        Assert.Equal(CodeViewLeafKind.Enum, _fixture.SimplePdb.Symbols.OfType<ConstantSymbol>().First().Type.LeafKind);
    }

    [Fact]
    public void Value()
    {
        Assert.Equal(2, _fixture.SimplePdb.Symbols.OfType<ConstantSymbol>().First().Value);
    }
}
