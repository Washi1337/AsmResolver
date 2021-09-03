using AsmResolver.PE.Imports;
using Xunit;

namespace AsmResolver.PE.Tests.Imports
{
    public class DefaultSymbolResolverTest
    {
        [Fact]
        public void ResolveWinsockSymbol()
        {
            var module = new ImportedModule("ws2_32.dll");
            var symbol = new ImportedSymbol(57);
            module.Symbols.Add(symbol);

            var resolvedSymbol = DefaultSymbolResolver.Instance.Resolve(symbol)!;
            Assert.NotNull(resolvedSymbol);
            Assert.Equal("gethostname", resolvedSymbol.Name);
        }

        [Fact]
        public void ResolveOleaut32Symbol()
        {
            var module = new ImportedModule("oleaut32.dll");
            var symbol = new ImportedSymbol(365);
            module.Symbols.Add(symbol);

            var resolvedSymbol = DefaultSymbolResolver.Instance.Resolve(symbol)!;
            Assert.NotNull(resolvedSymbol);
            Assert.Equal("VarDateFromUI8", resolvedSymbol.Name);
        }
    }
}
