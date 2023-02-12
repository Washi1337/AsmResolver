using System.Linq;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Records;
using Xunit;

namespace AsmResolver.Symbols.Pdb.Tests.Records;

public class FileStaticSymbolTest : IClassFixture<MockPdbFixture>
{
    private readonly FileStaticSymbol _symbol;

    public FileStaticSymbolTest(MockPdbFixture fixture)
    {
        _symbol = fixture.SimplePdb
            .Modules.First(m => m.Name!.Contains(@"msvcrt.nativeproj_110336922\objr\x86\dll_dllmain.obj"))
            .Symbols.OfType<ProcedureSymbol>().ElementAt(2)
            .Symbols.OfType<FileStaticSymbol>()
            .First();
    }

    [Fact]
    public void Name()
    {
        Assert.Equal("__proc_attached", _symbol.Name);
    }

    [Fact]
    public void Type()
    {
        Assert.Equal(SimpleTypeKind.Int32, Assert.IsAssignableFrom<SimpleTypeRecord>(_symbol.VariableType).Kind);
    }
}
