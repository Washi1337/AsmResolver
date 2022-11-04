using System.Linq;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class ProcedureReferenceSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public ProcedureReferenceSymbolTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Name()
    {
        Assert.Equal("DllMain", _fixture.SimplePdb.Symbols.OfType<ProcedureReferenceSymbol>().First().Name);
    }
}
