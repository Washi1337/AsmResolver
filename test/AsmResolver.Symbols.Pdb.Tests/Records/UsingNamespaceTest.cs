using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class UsingNamespaceTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public UsingNamespaceTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Name()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("argv_mode.obj"))
            .Symbols.OfType<UsingNamespaceSymbol>()
            .First();

        Assert.Equal("std", symbol.Name);
    }
}
