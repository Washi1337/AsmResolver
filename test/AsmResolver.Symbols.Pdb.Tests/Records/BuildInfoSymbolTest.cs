using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class BuildInfoSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public BuildInfoSymbolTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Info()
    {
        var symbol = _fixture.SimplePdb.Modules[1].Symbols.OfType<BuildInfoSymbol>().First();
        var info = symbol.Info;

        Assert.NotNull(info);
        Assert.Equal(5, info.Entries.Count);
    }
}
