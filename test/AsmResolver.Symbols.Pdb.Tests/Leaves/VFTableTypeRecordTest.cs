using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class VFTableTypeRecordTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public VFTableTypeRecordTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ReadBaseVFTableOwner()
    {
        // VTableBase has 3 virtual methods (vfunc1, vfunc2, ~VTableBase).
        var vftable = _fixture.SimplePdb.GetLeafRecords()
            .OfType<VFTableTypeRecord>()
            .FirstOrDefault(v => v.OwnerType is ClassTypeRecord c && c.Name == "VTableBase");
        Assert.NotNull(vftable);
        Assert.Equal("VTableBase", Assert.IsAssignableFrom<ClassTypeRecord>(vftable!.OwnerType).Name);
    }

    [Fact]
    public void ReadBaseVFTableHasNoBase()
    {
        var vftable = _fixture.SimplePdb.GetLeafRecords()
            .OfType<VFTableTypeRecord>()
            .First(v => v.OwnerType is ClassTypeRecord c && c.Name == "VTableBase");
        Assert.Null(vftable.BaseVFTable);
    }

    [Fact]
    public void ReadBaseVFTableOffset()
    {
        var vftable = _fixture.SimplePdb.GetLeafRecords()
            .OfType<VFTableTypeRecord>()
            .First(v => v.OwnerType is ClassTypeRecord c && c.Name == "VTableBase");
        Assert.Equal(0u, vftable.OffsetInObjectLayout);
    }

    [Fact]
    public void ReadBaseVFTableNames()
    {
        var vftable = _fixture.SimplePdb.GetLeafRecords()
            .OfType<VFTableTypeRecord>()
            .First(v => v.OwnerType is ClassTypeRecord c && c.Name == "VTableBase");
        // First name is the vtable name, rest are method names.
        Assert.True(vftable.Names.Count >= 2, $"Expected >=2 names, got {vftable.Names.Count}");
        Assert.Contains("VTableBase", vftable.Names[0]);
    }

    [Fact]
    public void ReadDerivedVFTableOwner()
    {
        var vftable = _fixture.SimplePdb.GetLeafRecords()
            .OfType<VFTableTypeRecord>()
            .FirstOrDefault(v => v.OwnerType is ClassTypeRecord c && c.Name == "VTableDerived");
        Assert.NotNull(vftable);
        Assert.Equal("VTableDerived", Assert.IsAssignableFrom<ClassTypeRecord>(vftable!.OwnerType).Name);
    }

    [Fact]
    public void ReadDerivedVFTableInheritsFromBase()
    {
        var vftable = _fixture.SimplePdb.GetLeafRecords()
            .OfType<VFTableTypeRecord>()
            .First(v => v.OwnerType is ClassTypeRecord c && c.Name == "VTableDerived");
        // VTableDerived overrides vfunc1 and adds vfunc3, so its VFTable should reference VTableBase's VFTable.
        Assert.NotNull(vftable.BaseVFTable);
    }

    [Fact]
    public void ReadDerivedVFTableHasMoreNames()
    {
        var baseVft = _fixture.SimplePdb.GetLeafRecords()
            .OfType<VFTableTypeRecord>()
            .First(v => v.OwnerType is ClassTypeRecord c && c.Name == "VTableBase");
        var derivedVft = _fixture.SimplePdb.GetLeafRecords()
            .OfType<VFTableTypeRecord>()
            .First(v => v.OwnerType is ClassTypeRecord c && c.Name == "VTableDerived");
        // Derived adds vfunc3 on top of base's methods.
        Assert.True(derivedVft.Names.Count >= baseVft.Names.Count,
            $"Derived has {derivedVft.Names.Count} names but base has {baseVft.Names.Count}");
    }
}
