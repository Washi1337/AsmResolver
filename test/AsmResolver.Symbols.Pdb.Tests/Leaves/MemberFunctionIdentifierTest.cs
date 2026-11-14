using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class MemberFunctionIdentifierTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public MemberFunctionIdentifierTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void CreateNewMemberFunctionIdentifier()
    {
        var parentType = new SimpleTypeRecord(SimpleTypeKind.Void);
        var funcType = new SimpleTypeRecord(SimpleTypeKind.Int32);
        var mfuncId = new MemberFunctionIdentifier(parentType, "TestFunc", funcType);
        Assert.Equal(CodeViewLeafKind.MFuncId, mfuncId.LeafKind);
        Assert.Equal("TestFunc", mfuncId.Name);
        Assert.Equal(SimpleTypeKind.Void, Assert.IsAssignableFrom<SimpleTypeRecord>(mfuncId.ParentType).Kind);
        Assert.Equal(SimpleTypeKind.Int32, Assert.IsAssignableFrom<SimpleTypeRecord>(mfuncId.FunctionType).Kind);
    }

    [Fact]
    public void ReadName()
    {
        var leaf = _fixture.SimplePdb.GetIdLeafRecord<MemberFunctionIdentifier>(0x14C9);
        Assert.Equal("configure_argv", leaf.Name);
    }

    [Fact]
    public void ReadParentType()
    {
        var leaf = _fixture.SimplePdb.GetIdLeafRecord<MemberFunctionIdentifier>(0x14C9);
        Assert.Equal("__scrt_narrow_argv_policy",
            Assert.IsAssignableFrom<ClassTypeRecord>(leaf.ParentType).Name);
    }

    [Fact]
    public void ReadNameFromMyTestApplication()
    {
        var leaf = _fixture.MyTestApplication.GetIdLeafRecord<MemberFunctionIdentifier>(0x104B);
        Assert.Equal("configure_argv", leaf.Name);
    }

    [Fact]
    public void ReadParentTypeFromMyTestApplication()
    {
        var leaf = _fixture.MyTestApplication.GetIdLeafRecord<MemberFunctionIdentifier>(0x104B);
        Assert.Equal("__scrt_narrow_argv_policy",
            Assert.IsAssignableFrom<ClassTypeRecord>(leaf.ParentType).Name);
    }

    [Fact]
    public void ReadMultipleMemberFunctionIdentifiers()
    {
        var leaf1 = _fixture.SimplePdb.GetIdLeafRecord<MemberFunctionIdentifier>(0x14C9);
        var leaf2 = _fixture.SimplePdb.GetIdLeafRecord<MemberFunctionIdentifier>(0x14CC);
        Assert.Equal("configure_argv", leaf1.Name);
        Assert.Equal("initialize_environment", leaf2.Name);
    }
}
