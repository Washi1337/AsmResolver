using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class ProcedureTypeTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public ProcedureTypeTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ReadReturnType()
    {
        var procedure = (ProcedureType) _fixture.SimplePdb.GetLeafRecord(0x18f7);
        Assert.Equal(SimpleTypeKind.Void, Assert.IsAssignableFrom<SimpleType>(procedure.ReturnType).Kind);
    }

    [Fact]
    public void ReadCallingConvention()
    {
        var procedure = (ProcedureType) _fixture.SimplePdb.GetLeafRecord(0x18f7);
        Assert.Equal(CodeViewCallingConvention.NearStd, procedure.CallingConvention);
    }

    [Fact]
    public void ReadArguments()
    {
        var procedure = (ProcedureType) _fixture.SimplePdb.GetLeafRecord(0x18f7);
        Assert.NotNull(procedure.Arguments);
        Assert.Equal(2, procedure.Arguments!.Types.Count);
    }
}
