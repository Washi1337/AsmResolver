using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class UdtModuleSourceLineLeafTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public UdtModuleSourceLineLeafTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ReadUdtType()
    {
        var leaf = _fixture.SimplePdb.GetIdLeafRecord<UdtModuleSourceLineLeaf>(0x100E);
        Assert.Equal("JOB_OBJECT_IO_RATE_CONTROL_FLAGS",
            Assert.IsAssignableFrom<EnumTypeRecord>(leaf.UdtType).Name);
    }

    [Fact]
    public void ReadLine()
    {
        var leaf = _fixture.SimplePdb.GetIdLeafRecord<UdtModuleSourceLineLeaf>(0x100E);
        Assert.Equal(12188u, leaf.Line);
    }

    [Fact]
    public void ReadModule()
    {
        var leaf = _fixture.SimplePdb.GetIdLeafRecord<UdtModuleSourceLineLeaf>(0x100E);
        Assert.Equal(3, leaf.Module);
    }

    [Fact]
    public void ReadSourceFileOffset()
    {
        var leaf = _fixture.SimplePdb.GetIdLeafRecord<UdtModuleSourceLineLeaf>(0x100E);
        Assert.Equal(312u, leaf.SourceFileOffset);
    }

    [Fact]
    public void ReadDifferentUdtType()
    {
        var leaf = _fixture.SimplePdb.GetIdLeafRecord<UdtModuleSourceLineLeaf>(0x1010);
        Assert.Equal("DISPLAYCONFIG_SCANLINE_ORDERING",
            Assert.IsAssignableFrom<EnumTypeRecord>(leaf.UdtType).Name);
    }

    [Fact]
    public void ReadDifferentLine()
    {
        var leaf = _fixture.SimplePdb.GetIdLeafRecord<UdtModuleSourceLineLeaf>(0x1010);
        Assert.Equal(2827u, leaf.Line);
    }

    [Fact]
    public void ReadDifferentSourceFileOffset()
    {
        var leaf = _fixture.SimplePdb.GetIdLeafRecord<UdtModuleSourceLineLeaf>(0x1010);
        Assert.Equal(383u, leaf.SourceFileOffset);
    }

    [Fact]
    public void ReadFromMyTestApplication()
    {
        var leaf = _fixture.MyTestApplication.GetIdLeafRecord<UdtModuleSourceLineLeaf>(0x1000);
        Assert.Equal("ReplacesCorHdrNumericDefines",
            Assert.IsAssignableFrom<EnumTypeRecord>(leaf.UdtType).Name);
        Assert.Equal(20620u, leaf.Line);
        Assert.Equal(3, leaf.Module);
        Assert.Equal(1u, leaf.SourceFileOffset);
    }

    [Fact]
    public void CreateNewUdtModuleSourceLine()
    {
        var type = new SimpleTypeRecord(SimpleTypeKind.Int32);
        var leaf = new UdtModuleSourceLineLeaf(type, 100, 42, 5);
        Assert.Equal(CodeViewLeafKind.UdtModSrcLine, leaf.LeafKind);
        Assert.Equal(SimpleTypeKind.Int32, Assert.IsAssignableFrom<SimpleTypeRecord>(leaf.UdtType).Kind);
        Assert.Equal(100u, leaf.SourceFileOffset);
        Assert.Equal(42u, leaf.Line);
        Assert.Equal(5, leaf.Module);
    }
}
