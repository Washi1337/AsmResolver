using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;
using static AsmResolver.Symbols.Pdb.Leaves.CodeViewFieldAttributes;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class FieldListLeafTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public FieldListLeafTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ReadEnumerateList()
    {
        var list = (FieldListLeaf) _fixture.SimplePdb.GetLeafRecord(0x1008);
        var enumerates = list.Entries
            .Cast<EnumerateField>()
            .Select(f => (f.Attributes, f.Name.Value, f.Value))
            .ToArray();

        Assert.Equal((Public, "DISPLAYCONFIG_SCANLINE_ORDERING_UNSPECIFIED", 0u), enumerates[0]);
        Assert.Equal((Public, "DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE", 1u), enumerates[1]);
        Assert.Equal((Public, "DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED", 2u), enumerates[2]);
        Assert.Equal((Public, "DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_UPPERFIELDFIRST", 2u), enumerates[3]);
        Assert.Equal((Public, "DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_LOWERFIELDFIRST", 3u), enumerates[4]);
        Assert.Equal((Public, "DISPLAYCONFIG_SCANLINE_ORDERING_FORCE_UINT32", '\xff'), enumerates[5]);
    }

    [Fact]
    public void ReadInstanceDataMemberList()
    {
        var list = (FieldListLeaf) _fixture.SimplePdb.GetLeafRecord(0x1017);
        var enumerates = list.Entries
            .Cast<InstanceDataField>()
            .Select(f => (f.Attributes, f.Name.Value, f.Offset))
            .ToArray();

        Assert.Equal((Public, "cbSize", 0ul), enumerates[0]);
        Assert.Equal((Public, "fMask", 4ul), enumerates[1]);
        Assert.Equal((Public, "fType", 8ul), enumerates[2]);
        Assert.Equal((Public, "fState", 12ul), enumerates[3]);
        Assert.Equal((Public, "wID", 16ul), enumerates[4]);
        Assert.Equal((Public, "hSubMenu", 20ul), enumerates[5]);
        Assert.Equal((Public, "hbmpChecked", 24ul), enumerates[6]);
        Assert.Equal((Public, "hbmpUnchecked", 28ul), enumerates[7]);
        Assert.Equal((Public, "dwItemData", 32ul), enumerates[8]);
        Assert.Equal((Public, "dwTypeData", 36ul), enumerates[9]);
        Assert.Equal((Public, "cch", 40ul), enumerates[10]);
        Assert.Equal((Public, "hbmpItem", 44ul), enumerates[11]);
    }

    [Fact]
    public void ReadMethodsAndBaseClass()
    {
        var list = (FieldListLeaf) _fixture.SimplePdb.GetLeafRecord(0x239d);

        Assert.Equal("std::exception", Assert.IsAssignableFrom<ClassTypeRecord>(
            Assert.IsAssignableFrom<BaseClassField>(list.Entries[0]).Type).Name);
        Assert.Equal("bad_cast", Assert.IsAssignableFrom<OverloadedMethod>(list.Entries[1]).Name);
        Assert.Equal("__construct_from_string_literal", Assert.IsAssignableFrom<NonOverloadedMethod>(list.Entries[2]).Name);
        Assert.Equal("~bad_cast", Assert.IsAssignableFrom<NonOverloadedMethod>(list.Entries[3]).Name);
        Assert.Equal("operator=", Assert.IsAssignableFrom<OverloadedMethod>(list.Entries[4]).Name);
        Assert.Equal("__local_vftable_ctor_closure", Assert.IsAssignableFrom<NonOverloadedMethod>(list.Entries[5]).Name);
        Assert.Equal("__vecDelDtor", Assert.IsAssignableFrom<NonOverloadedMethod>(list.Entries[6]).Name);
    }

    [Fact]
    public void ReadNestedTypes()
    {
        var list = (FieldListLeaf) _fixture.SimplePdb.GetLeafRecord(0x1854);

        Assert.Equal("_LDT_ENTRY::<unnamed-type-HighWord>::<unnamed-type-Bytes>",
            Assert.IsAssignableFrom<ClassTypeRecord>(Assert.IsAssignableFrom<NestedTypeField>(list.Entries[0]).Type).Name);
        Assert.Equal("_LDT_ENTRY::<unnamed-type-HighWord>::<unnamed-type-Bits>",
            Assert.IsAssignableFrom<ClassTypeRecord>(Assert.IsAssignableFrom<NestedTypeField>(list.Entries[2]).Type).Name);
    }

    [Fact]
    public void ReadVirtualBaseClass()
    {
        var list = (FieldListLeaf) _fixture.MyTestApplication.GetLeafRecord(0x1347);
        var baseClass = Assert.IsAssignableFrom<VirtualBaseClassField>(list.Entries[0]);

        Assert.Equal("std::basic_ios<char,std::char_traits<char> >",
            Assert.IsAssignableFrom<ClassTypeRecord>(baseClass.Type).Name);
        Assert.True(Assert.IsAssignableFrom<PointerTypeRecord>(baseClass.PointerType).IsNear64);
        Assert.False(baseClass.IsIndirect);
        Assert.Equal(0ul, baseClass.PointerOffset);
        Assert.Equal(1ul, baseClass.TableOffset);
    }

    [Fact]
    public void ReadIndirectVirtualBaseClass()
    {
        var list = (FieldListLeaf) _fixture.MyTestApplication.GetLeafRecord(0x1e97);
        var baseClass = Assert.IsAssignableFrom<VirtualBaseClassField>(list.Entries[2]);

        Assert.Equal("std::basic_ios<char,std::char_traits<char> >",
            Assert.IsAssignableFrom<ClassTypeRecord>(baseClass.Type).Name);
        Assert.True(Assert.IsAssignableFrom<PointerTypeRecord>(baseClass.PointerType).IsNear64);
        Assert.True(baseClass.IsIndirect);
        Assert.Equal(0ul, baseClass.PointerOffset);
        Assert.Equal(1ul, baseClass.TableOffset);
    }
}
