using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class CompileSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly Compile2Symbol _compile2Symbol;
    private readonly Compile3Symbol _compile3Symbol;

    public CompileSymbolTest(MockPdbFixture fixture)
    {
        var kernel32Module = fixture.SimplePdb.Modules.First(m => m.Name == "KERNEL32.dll");
        var dllmainModule = fixture.SimplePdb.Modules.First(m =>
            m.Name == @"C:\Users\Admin\source\repos\AsmResolver\test\TestBinaries\Native\SimpleDll\Release\dllmain.obj");

        _compile2Symbol = kernel32Module.Symbols.OfType<Compile2Symbol>().First();
        _compile3Symbol = dllmainModule.Symbols.OfType<Compile3Symbol>().First();
    }

    [Fact]
    public void Flags2()
    {
        Assert.Equal(SourceLanguage.Link, _compile2Symbol.Language);
        Assert.Equal(CompileAttributes.None, _compile2Symbol.Attributes);
    }

    [Fact]
    public void Flags3()
    {
        Assert.Equal(SourceLanguage.Cpp, _compile3Symbol.Language);
        Assert.Equal(
            CompileAttributes.Sdl | CompileAttributes.SecurityChecks | CompileAttributes.Ltcg,
            _compile3Symbol.Attributes);
    }

    [Fact]
    public void Machine2()
    {
        Assert.Equal(CpuType.Intel80386, _compile2Symbol.Machine);
    }

    [Fact]
    public void Machine3()
    {
        Assert.Equal(CpuType.Pentium3, _compile3Symbol.Machine);
    }

    [Fact]
    public void NumericVersions2()
    {
        Assert.Equal(0, _compile2Symbol.FrontEndMajorVersion);
        Assert.Equal(0, _compile2Symbol.FrontEndMinorVersion);
        Assert.Equal(0, _compile2Symbol.FrontEndBuildVersion);
        Assert.Equal(14, _compile2Symbol.BackEndMajorVersion);
        Assert.Equal(20, _compile2Symbol.BackEndMinorVersion);
        Assert.Equal(27412, _compile2Symbol.BackEndBuildVersion);
    }

    [Fact]
    public void NumericVersions3()
    {
        Assert.Equal(19, _compile3Symbol.FrontEndMajorVersion);
        Assert.Equal(29, _compile3Symbol.FrontEndMinorVersion);
        Assert.Equal(30143, _compile3Symbol.FrontEndBuildVersion);
        Assert.Equal(0, _compile3Symbol.FrontEndQfeVersion);
        Assert.Equal(19, _compile3Symbol.BackEndMajorVersion);
        Assert.Equal(29, _compile3Symbol.BackEndMinorVersion);
        Assert.Equal(30143, _compile3Symbol.BackEndBuildVersion);
        Assert.Equal(0, _compile3Symbol.BackEndQfeVersion);
    }

    [Fact]
    public void CompilerVersionString2()
    {
        Assert.Equal("Microsoft (R) LINK", _compile2Symbol.CompilerVersion);
    }

    [Fact]
    public void CompilerVersionString3()
    {
        Assert.Equal("Microsoft (R) Optimizing Compiler", _compile3Symbol.CompilerVersion);
    }
}
