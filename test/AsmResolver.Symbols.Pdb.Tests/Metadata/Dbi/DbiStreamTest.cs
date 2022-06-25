using System;
using System.IO;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.File.Headers;
using AsmResolver.Symbols.Pdb.Metadata.Dbi;
using AsmResolver.Symbols.Pdb.Msf;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Metadata.Dbi;

public class DbiStreamTest
{
    private DbiStream GetDbiStream(bool rebuild)
    {
        var file = MsfFile.FromBytes(Properties.Resources.SimpleDllPdb);
        var dbiStream = DbiStream.FromReader(file.Streams[DbiStream.StreamIndex].CreateReader());

        if (rebuild)
        {
            using var stream = new MemoryStream();
            dbiStream.Write(new BinaryStreamWriter(stream));
            dbiStream = DbiStream.FromReader(ByteArrayDataSource.CreateReader(stream.ToArray()));
        }

        return dbiStream;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Header(bool rebuild)
    {
        var dbiStream = GetDbiStream(rebuild);

        Assert.Equal(1u, dbiStream.Age);
        Assert.Equal(DbiAttributes.None, dbiStream.Attributes);
        Assert.Equal(MachineType.I386, dbiStream.Machine);
        Assert.Equal(14, dbiStream.BuildMajorVersion);
        Assert.Equal(29, dbiStream.BuildMinorVersion);
        Assert.True(dbiStream.IsNewVersionFormat);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ModuleNames(bool rebuild)
    {
        var dbiStream = GetDbiStream(rebuild);

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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SectionContributions(bool rebuild)
    {
        var dbiStream = GetDbiStream(rebuild);

        Assert.Equal(new (ushort, uint)[]
        {
            (1, 1669053862), (16, 2162654757), (20, 1635644926), (20, 3159649454), (20, 1649652954), (20, 3877379438),
            (20, 4262788820), (20, 199934614), (8, 4235719287), (8, 1374843914), (9, 4241735292), (9, 2170796787),
            (19, 1300950661), (19, 3968158929), (18, 3928463356), (18, 3928463356), (18, 2109213706), (22, 1457516325),
            (22, 3939645857), (22, 1393694582), (22, 546064581), (22, 1976627334), (22, 513172946), (22, 25744891),
            (22, 1989765812), (22, 2066266302), (22, 3810887196), (22, 206965504), (22, 647717352), (22, 3911072265),
            (22, 3290064241), (12, 3928463356), (24, 2717331243), (24, 3687876222), (25, 2318145338), (25, 2318145338),
            (6, 542071654), (15, 1810708069), (10, 3974941622), (14, 1150179208), (17, 2709606169), (13, 2361171624),
            (28, 0), (28, 0), (28, 0), (29, 0), (29, 0), (29, 0), (29, 0), (29, 0), (29, 0), (29, 0), (29, 0),
            (23, 3467414241), (23, 4079273803), (26, 1282639619), (4, 0), (4, 0), (4, 0), (4, 0), (4, 0), (4, 0),
            (4, 0), (4, 0), (4, 0), (4, 0), (4, 0), (5, 0), (28, 0), (28, 0), (28, 0), (27, 0), (29, 0), (29, 0),
            (29, 0), (29, 0), (29, 0), (29, 0), (29, 0), (29, 0), (30, 0), (10, 2556510175), (21, 2556510175),
            (21, 2556510175), (21, 2556510175), (21, 2556510175), (21, 2556510175), (21, 2556510175), (21, 2556510175),
            (21, 2556510175), (20, 2556510175), (8, 4117779887), (31, 0), (11, 525614319), (31, 0), (31, 0), (31, 0),
            (31, 0), (31, 0), (25, 2556510175), (25, 2556510175), (25, 2556510175), (25, 2556510175), (20, 3906165615),
            (20, 1185345766), (20, 407658226), (22, 2869884627), (27, 0), (30, 0), (5, 0), (27, 0), (4, 0), (4, 0),
            (4, 0), (4, 0), (4, 0), (4, 0), (4, 0), (4, 0), (4, 0), (4, 0), (4, 0), (5, 0), (28, 0), (28, 0), (28, 0),
            (27, 0), (29, 0), (29, 0), (29, 0), (29, 0), (29, 0), (29, 0), (29, 0), (29, 0), (30, 0), (28, 0), (28, 0),
            (28, 0), (27, 0), (29, 0), (29, 0), (29, 0), (29, 0), (29, 0), (29, 0), (29, 0), (29, 0), (30, 0), (4, 0),
            (4, 0), (4, 0), (4, 0), (4, 0), (4, 0), (4, 0), (4, 0), (4, 0), (4, 0), (4, 0), (5, 0), (7, 4096381681),
            (22, 454268333), (14, 1927129959), (23, 1927129959), (20, 0), (8, 0), (19, 0), (18, 0), (18, 0), (22, 0),
            (24, 0), (10, 0), (14, 0), (2, 0), (31, 0), (3, 0), (3, 0)
        }, dbiStream.SectionContributions.Select(x => (x.ModuleIndex, x.DataCrc)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SectionMaps(bool rebuild)
    {
        var dbiStream = GetDbiStream(rebuild);

        Assert.Equal(new (ushort, ushort, ushort, ushort, ushort, ushort, uint, uint)[]
            {
                (0x010d, 0x0000, 0x0000, 0x0001, 0xffff, 0xffff, 0x00000000, 0x00000ce8),
                (0x0109, 0x0000, 0x0000, 0x0002, 0xffff, 0xffff, 0x00000000, 0x00000834),
                (0x010b, 0x0000, 0x0000, 0x0003, 0xffff, 0xffff, 0x00000000, 0x00000394),
                (0x0109, 0x0000, 0x0000, 0x0004, 0xffff, 0xffff, 0x00000000, 0x000000f8),
                (0x0109, 0x0000, 0x0000, 0x0005, 0xffff, 0xffff, 0x00000000, 0x0000013c),
                (0x0208, 0x0000, 0x0000, 0x0000, 0xffff, 0xffff, 0x00000000, 0xffffffff),
            },
            dbiStream.SectionMaps.Select(m => ((ushort)
                m.Attributes, m.LogicalOverlayNumber, m.Group, m.Frame,
                m.SectionName, m.ClassName, m.Offset, m.SectionLength)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SourceFiles(bool rebuild)
    {
        var dbiStream = GetDbiStream(rebuild);

        string[][] firstThreeActualFileLists = dbiStream.SourceFiles
            .Take(3)
            .Select(x => x
                .Select(y => y.ToString())
                .ToArray()
            ).ToArray();

        Assert.Equal(new[]
            {
                Array.Empty<string>(),
                new[]
                {
                    @"C:\Users\Admin\source\repos\AsmResolver\test\TestBinaries\Native\SimpleDll\pch.h",
                    @"C:\Users\Admin\source\repos\AsmResolver\test\TestBinaries\Native\SimpleDll\dllmain.cpp",
                    @"C:\Users\Admin\source\repos\AsmResolver\test\TestBinaries\Native\SimpleDll\Release\SimpleDll.pch",
                },
                new[]
                {
                    @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\um\winuser.h",
                    @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\shared\basetsd.h",
                    @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\um\winbase.h",
                    @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\shared\stralign.h",
                    @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\shared\guiddef.h",
                    @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\shared\winerror.h",
                    @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\ucrt\corecrt_wstring.h",
                    @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\um\processthreadsapi.h",
                    @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\um\winnt.h",
                    @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\ucrt\ctype.h",
                    @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\ucrt\string.h",
                    @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\ucrt\corecrt_memory.h",
                    @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\um\memoryapi.h",
                    @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\ucrt\corecrt_memcpy_s.h",
                    @"C:\Users\Admin\source\repos\AsmResolver\test\TestBinaries\Native\SimpleDll\Release\SimpleDll.pch",
                }
            },
            firstThreeActualFileLists);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExtraDebugIndices(bool rebuild)
    {
        var dbiStream = GetDbiStream(rebuild);

        Assert.Equal(new ushort[]
        {
            0x7, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xB, 0xFFFF, 0xFFFF, 0xFFFF, 0xD, 0xFFFF
        }, dbiStream.ExtraStreamIndices);
    }

    [Fact]
    public void SizeCalculation()
    {
        var file = MsfFile.FromBytes(Properties.Resources.SimpleDllPdb);
        var infoStream = DbiStream.FromReader(file.Streams[DbiStream.StreamIndex].CreateReader());

        uint calculatedSize = infoStream.GetPhysicalSize();

        using var stream = new MemoryStream();
        infoStream.Write(new BinaryStreamWriter(stream));

        Assert.Equal(stream.Length, calculatedSize);
    }
}
