using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class EnvironmentBlockTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public EnvironmentBlockTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Entries()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name == "* Linker *")
            .Symbols.OfType<EnvironmentBlockSymbol>()
            .First();

        Assert.Equal(new[]
        {
            ("cwd", @"C:\Users\Admin\source\repos\AsmResolver\test\TestBinaries\Native\SimpleDll"),
            ("exe",
                @"C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.29.30133\bin\HostX86\x86\link.exe"),
            ("pdb", @"C:\Users\Admin\source\repos\AsmResolver\Release\SimpleDll.pdb"),
            ("cmd",
                " /ERRORREPORT:PROMPT /OUT:C:\\Users\\Admin\\source\\repos\\AsmResolver\\Release\\SimpleDll.dll /INCREMENTAL:NO /NOLOGO /MANIFEST /MANIFESTUAC:NO /manifest:embed /DEBUG /PDB:C:\\Users\\Admin\\source\\repos\\AsmResolver\\Release\\SimpleDll.pdb /SUBSYSTEM:WINDOWS /OPT:REF /OPT:ICF /LTCG:incremental /LTCGOUT:Release\\SimpleDll.iobj /TLBID:1 /DYNAMICBASE /NXCOMPAT /IMPLIB:C:\\Users\\Admin\\source\\repos\\AsmResolver\\Release\\SimpleDll.lib /MACHINE:X86 /SAFESEH /DLL"),
        }, symbol.Entries.Select(x => (x.Key.Value, x.Value.Value)));
    }
}
