using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class BuildInfoLeafTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public BuildInfoLeafTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Entries()
    {
        var info = _fixture.SimplePdb.GetIdLeafRecord<BuildInfoLeaf>(0x100d);

        Assert.Equal(new[]
        {
            @"C:\Users\Admin\source\repos\AsmResolver\test\TestBinaries\Native\SimpleDll",
            @"C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.29.30133\bin\HostX86\x86\CL.exe",
            @"dllmain.cpp",
            @"C:\Users\Admin\source\repos\AsmResolver\test\TestBinaries\Native\SimpleDll\Release\vc142.pdb",
            @" Files (x86)\Windows Kits\10\Include\10.0.19041.0\winrt"" -external:I""C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\cppwinrt"" -external:I""C:\Program Files (x86)\Windows Kits\NETFXSDK\4.8\Include\um"" -X"
        }, info.Entries.Select(e => e.Value.Value));
    }
}
