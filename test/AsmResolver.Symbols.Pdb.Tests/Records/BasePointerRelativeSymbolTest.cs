using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class BasePointerRelativeSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly BasePointerRelativeSymbol _symbol;

    public BasePointerRelativeSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains("gs_report.obj"))
            .Symbols.OfType<ProcedureSymbol>().First()
            .Symbols.OfType<BasePointerRelativeSymbol>()
            .First();
    }

    [Fact]
    public void Name()
    {
        Assert.Equal("exception_pointers", _symbol.Name);
    }

    [Fact]
    public void Offset()
    {
        Assert.Equal(8, _symbol.Offset);
    }

    [Fact]
    public void Type()
    {
        Assert.IsAssignableFrom<PointerTypeRecord>(_symbol.VariableType);
    }

}
