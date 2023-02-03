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
}
