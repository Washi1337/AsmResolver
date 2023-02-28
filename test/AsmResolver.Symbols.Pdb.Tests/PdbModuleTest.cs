using System.Collections.Generic;
using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests;

public class PdbModuleTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public PdbModuleTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Name()
    {
        Assert.Equal("* CIL *", _fixture.SimplePdb.Modules[0].Name);
    }

    [Fact]
    public void ObjectFileName()
    {
        var module = _fixture.SimplePdb.Modules[4];
        Assert.Equal("Import:KERNEL32.dll", module.Name);
        Assert.Equal(
            @"C:\Program Files (x86)\Windows Kits\10\lib\10.0.19041.0\um\x86\kernel32.lib",
            module.ObjectFileName);
    }

    [Fact]
    public void Children()
    {
        var module = _fixture.SimplePdb.Modules.First(m => m.Name!.Contains("dllmain.obj"));
        Assert.Equal(new[]
        {
            CodeViewSymbolType.ObjName,
            CodeViewSymbolType.Compile3,
            CodeViewSymbolType.BuildInfo,
            CodeViewSymbolType.GProc32,
        }, module.Symbols.Select(x => x.CodeViewSymbolType));
    }

    [Fact]
    public void SourceFiles()
    {
        var module = _fixture.SimplePdb.Modules.First(m => m.Name!.Contains("dllmain.obj"));
        Assert.Equal(new Utf8String[]
        {
            "C:\\Users\\Admin\\source\\repos\\AsmResolver\\test\\TestBinaries\\Native\\SimpleDll\\pch.h",
            "C:\\Users\\Admin\\source\\repos\\AsmResolver\\test\\TestBinaries\\Native\\SimpleDll\\dllmain.cpp",
            "C:\\Users\\Admin\\source\\repos\\AsmResolver\\test\\TestBinaries\\Native\\SimpleDll\\Release\\SimpleDll.pch"
        }, module.SourceFiles);
    }
}
