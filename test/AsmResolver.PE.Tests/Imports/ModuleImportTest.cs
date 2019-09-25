using System.Linq;
using Xunit;

namespace AsmResolver.PE.Tests.Imports
{
    public class ModuleImportTest
    {
        [Fact]
        public void ReadDotNetHelloWorld()
        {
            var peImage = PEImageBase.FromBytes(Properties.Resources.HelloWorld);
            
            Assert.Single(peImage.Imports);
            Assert.Equal("mscoree.dll", peImage.Imports[0].Name);
            Assert.Single(peImage.Imports[0].Members);
            Assert.Equal("_CorExeMain", peImage.Imports[0].Members[0].Name);
        }

        [Fact]
        public void ReadSimpleDll()
        {
            var peImage = PEImageBase.FromBytes(Properties.Resources.SimpleDll);
            
            var module = peImage.Imports.First(m => m.Name == "ucrtbased.dll");
            Assert.NotNull(module);
            Assert.Contains("__stdio_common_vsprintf_s", module.Members.Select(m => m.Name));
        }
        
    }
}