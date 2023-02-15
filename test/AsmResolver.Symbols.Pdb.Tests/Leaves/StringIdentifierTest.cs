using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Leaves;

public class StringIdentifierTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public StringIdentifierTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Value()
    {
        var id = _fixture.SimplePdb.GetIdLeafRecord<StringIdentifier>(0x1000);
        Assert.Equal(@"C:\Users\Admin\source\repos\AsmResolver\test\TestBinaries\Native\SimpleDll", id.Value);
    }

    [Fact]
    public void NoSubStrings()
    {
        var id = _fixture.SimplePdb.GetIdLeafRecord<StringIdentifier>(0x1000);
        Assert.Null(id.SubStrings);
    }

    [Fact]
    public void SubStrings()
    {
        var id = _fixture.SimplePdb.GetIdLeafRecord<StringIdentifier>(0x100c);
        var subStrings = id.SubStrings;

        Assert.NotNull(subStrings);
        Assert.Equal(new[]
        {
            @"-c -Zi -nologo -W3 -WX- -diagnostics:column -sdl -O2 -Oi -Oy- -GL -DWIN32 -DNDEBUG -DSIMPLEDLL_EXPORTS -D_WINDOWS -D_USRDLL -D_WINDLL -D_UNICODE -DUNICODE -Gm- -EHs -EHc -MD -GS -Gy -fp:precise -permissive- -Zc:wchar_t -Zc:forScope",
            @" -Zc:inline -Yupch.h -FpC:\Users\Admin\source\repos\AsmResolver\test\TestBinaries\Native\SimpleDll\Release\SimpleDll.pch -external:W3 -Gd -TP -analyze- -FC -errorreport:prompt -I""C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.29.3013",
            @"3\include"" -I""C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.29.30133\atlmfc\include"" -I""C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\VS\include"" -I""C:\Program Files (x86)\Windows",
            @" Kits\10\Include\10.0.19041.0\ucrt"" -I""C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\um"" -I""C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\shared"" -I""C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\winrt""",
            @" -I""C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\cppwinrt"" -I""C:\Program Files (x86)\Windows Kits\NETFXSDK\4.8\Include\um"" -external:I""C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.29.30133\include""",
            @" -external:I""C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.29.30133\atlmfc\include"" -external:I""C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\VS\include"" -external:I""C:\Program Files",
            @" (x86)\Windows Kits\10\Include\10.0.19041.0\ucrt"" -external:I""C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\um"" -external:I""C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\shared"" -external:I""C:\Program",
        }, subStrings.Entries.Select(x => x.Value.Value));
    }
}
