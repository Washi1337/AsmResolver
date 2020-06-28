using System.Linq;
using Xunit;

namespace AsmResolver.PE.Tests.Imports
{
    public class ModuleImportTest
    {
        [Fact]
        public void ReadDotNetHelloWorld()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            
            Assert.Single(peImage.Imports);
            Assert.Equal("mscoree.dll", peImage.Imports[0].Name);
            Assert.Single(peImage.Imports[0].Symbols);
            Assert.Equal("_CorExeMain", peImage.Imports[0].Symbols[0].Name);
        }

        [Fact]
        public void ReadSimpleDll()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.SimpleDll);
            
            var module = peImage.Imports.First(m => m.Name == "ucrtbased.dll");
            Assert.NotNull(module);
            Assert.Contains("__stdio_common_vsprintf_s", module.Symbols.Select(m => m.Name));
        }
        
    }
}