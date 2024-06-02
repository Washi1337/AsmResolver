using System;
using System.Linq;
using AsmResolver.PE.File;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests;

public class PdbImageTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public PdbImageTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void BasicMetadata()
    {
        var image = _fixture.SimplePdb;

        Assert.Equal(1u, image.Age);
        Assert.Equal(Guid.Parse("205dc366-d8f8-4175-8e06-26dd76722df5"), image.UniqueId);
        Assert.Equal(DbiAttributes.None, image.Attributes);
        Assert.Equal(MachineType.I386, image.Machine);
        Assert.Equal(14, image.BuildMajorVersion);
        Assert.Equal(29, image.BuildMinorVersion);
    }

    [Theory]
    [InlineData(0x00_75, SimpleTypeKind.UInt32, SimpleTypeMode.Direct)]
    [InlineData(0x04_03, SimpleTypeKind.Void, SimpleTypeMode.NearPointer32)]
    public void SimpleTypeLookup(uint typeIndex, SimpleTypeKind kind, SimpleTypeMode mode)
    {
        var type = Assert.IsAssignableFrom<SimpleTypeRecord>(_fixture.SimplePdb.GetLeafRecord(typeIndex));
        Assert.Equal(kind, type.Kind);
        Assert.Equal(mode, type.Mode);
    }

    [Fact]
    public void SimpleTypeLookupTwiceShouldCache()
    {
        var image = _fixture.SimplePdb;

        var type = image.GetLeafRecord(0x00_75);
        var type2 = image.GetLeafRecord(0x00_75);

        Assert.Same(type, type2);
    }

    [Fact]
    public void ReadModules()
    {
        Assert.Equal(new[]
        {
            @"* CIL *",
            @"C:\Users\Admin\source\repos\AsmResolver\test\TestBinaries\Native\SimpleDll\Release\dllmain.obj",
            @"C:\Users\Admin\source\repos\AsmResolver\test\TestBinaries\Native\SimpleDll\Release\pch.obj",
            @"* Linker Generated Manifest RES *",
            @"Import:KERNEL32.dll",
            @"KERNEL32.dll",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\sehprolg4.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\gs_cookie.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\gs_report.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\gs_support.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\guard_support.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\loadcfg.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\dyn_tls_init.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\ucrt_detection.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\cpu_disp.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\chandler4gs.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\secchk.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\argv_mode.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\default_local_stdio_options.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\tncleanup.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\dll_dllmain.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\initializers.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\utility.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\ucrt_stubs.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\utility_desktop.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\initsect.obj",
            @"D:\a\_work\1\s\Intermediate\vctools\msvcrt.nativeproj_110336922\objr\x86\x86_exception_filter.obj",
            @"VCRUNTIME140.dll",
            @"Import:VCRUNTIME140.dll",
            @"Import:api-ms-win-crt-runtime-l1-1-0.dll",
            @"api-ms-win-crt-runtime-l1-1-0.dll",
            "* Linker *"
        }, _fixture.SimplePdb.Modules.Select(m => m.Name!.Value));
    }
}
