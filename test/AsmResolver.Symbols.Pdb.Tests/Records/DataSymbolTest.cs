using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class DataSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly MockPdbFixture _fixture;

    public DataSymbolTest(MockPdbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Global()
    {
        var symbol = _fixture.SimplePdb.Symbols.OfType<DataSymbol>().First();

        Assert.True(symbol.IsGlobal);
        Assert.Equal(CodeViewSymbolType.GData32, symbol.CodeViewSymbolType);
    }

    [Fact]
    public void Local()
    {
        var symbol = _fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("gs_report"))
            .Symbols.OfType<DataSymbol>()
            .First();

        Assert.True(symbol.IsLocal);
        Assert.Equal(CodeViewSymbolType.LData32, symbol.CodeViewSymbolType);
    }

    [Fact]
    public void BasicProperties()
    {
        var symbol = _fixture.SimplePdb.Symbols.OfType<DataSymbol>().First();

        Assert.Equal(3, symbol.SegmentIndex);
        Assert.Equal(0x38Cu, symbol.Offset);
        Assert.Equal(SimpleTypeKind.Int32, Assert.IsAssignableFrom<SimpleTypeRecord>(symbol.VariableType).Kind);
    }

    [Fact]
    public void Name()
    {
        var symbol = _fixture.SimplePdb.Symbols.OfType<DataSymbol>().First();

        Assert.Equal(
            "__@@_PchSym_@00@UfhvihUzwnrmUhlfixvUivklhUzhnivhloeviUgvhgUgvhgyrmzirvhUmzgrevUhrnkovwooUivovzhvUkxsOlyq@4B2008FD98C1DD4",
            symbol.Name);
    }
}
