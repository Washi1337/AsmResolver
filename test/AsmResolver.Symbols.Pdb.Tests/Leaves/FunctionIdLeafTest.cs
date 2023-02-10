using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class FunctionIdLeafTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public FunctionIdLeafTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Name()
    {
        var leaf = _fixture.SimplePdb.GetIdLeafRecord<FunctionIdLeaf>(0x1453);
        Assert.Equal("__get_entropy", leaf.Name);
    }

    [Fact]
    public void FunctionType()
    {
        var leaf = _fixture.SimplePdb.GetIdLeafRecord<FunctionIdLeaf>(0x1453);
        Assert.IsAssignableFrom<ProcedureTypeRecord>(leaf.FunctionType);
    }
}
