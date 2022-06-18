using System.Linq;
using AsmResolver.PE.File.Headers;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;
using AsmResolver.Symbols.Pdb.Msf;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Metadata.Dbi;

public class DbiStreamTest
{
    [Fact]
    public void ReadHeader()
    {
        var file = MsfFile.FromBytes(Properties.Resources.SimpleDllPdb);
        var dbiStream = DbiStream.FromReader(file.Streams[DbiStream.StreamIndex].CreateReader());

        Assert.Equal(1u, dbiStream.Age);
        Assert.Equal(DbiAttributes.None, dbiStream.Attributes);
        Assert.Equal(MachineType.I386, dbiStream.Machine);
    }

    [Fact]
    public void ReadModuleNames()
    {
        var file = MsfFile.FromBytes(Properties.Resources.SimpleDllPdb);
        var dbiStream = DbiStream.FromReader(file.Streams[DbiStream.StreamIndex].CreateReader());

        Assert.Equal(new[]
        {
            "* CIL *",
            "C:\\Users\\Admin\\source\\repos\\AsmResolver\\test\\TestBinaries\\Native\\SimpleDll\\Release\\dllmain.obj",
            "C:\\Users\\Admin\\source\\repos\\AsmResolver\\test\\TestBinaries\\Native\\SimpleDll\\Release\\pch.obj",
            "* Linker Generated Manifest RES *",
            "Import:KERNEL32.dll",
            "KERNEL32.dll",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\sehprolg4.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\gs_cookie.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\gs_report.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\gs_support.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\guard_support.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\loadcfg.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\dyn_tls_init.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\ucrt_detection.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\cpu_disp.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\chandler4gs.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\secchk.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\argv_mode.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\default_local_stdio_options.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\tncleanup.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\dll_dllmain.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\initializers.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\utility.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\ucrt_stubs.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\utility_desktop.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\initsect.obj",
            "D:\\a\\_work\\1\\s\\Intermediate\\vctools\\msvcrt.nativeproj_110336922\\objr\\x86\\x86_exception_filter.obj",
            "VCRUNTIME140.dll",
            "Import:VCRUNTIME140.dll",
            "Import:api-ms-win-crt-runtime-l1-1-0.dll",
            "api-ms-win-crt-runtime-l1-1-0.dll",
            "* Linker *",
        }, dbiStream.Modules.Select(m => m.ModuleName?.Value));
    }
}
