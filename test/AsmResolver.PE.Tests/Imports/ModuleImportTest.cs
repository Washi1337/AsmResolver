using System.Linq;
using Xunit;

namespace AsmResolver.PE.Tests.Imports
{
    public class ModuleImportTest
    {
        [Fact]
        public void ReadDotNetHelloWorld()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);

            Assert.Single(peImage.Imports);
            Assert.Equal("mscoree.dll", peImage.Imports[0].Name);
            Assert.Single(peImage.Imports[0].Symbols);
            Assert.Equal("_CorExeMain", peImage.Imports[0].Symbols[0].Name);
        }

        [Fact]
        public void ReadSimpleDll()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.SimpleDll, TestReaderParameters);

            var module = peImage.Imports.First(m => m.Name == "ucrtbased.dll");
            Assert.NotNull(module);
            Assert.Contains("__stdio_common_vsprintf_s", module.Symbols.Select(m => m.Name));
        }

        [Fact]
        public void ReadTableWithEmptyOriginalFirstThunk()
        {
            // https://github.com/Washi1337/AsmResolver/issues/431

            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld_UPX, TestReaderParameters);

            var module = peImage.Imports.First(m => m.Name == "KERNEL32.DLL");
            Assert.NotNull(module);

            Assert.Equal(new[]
            {
                "LoadLibraryA",
                "ExitProcess",
                "GetProcAddress",
                "VirtualProtect",
            }, module.Symbols.Select(x => x.Name));
        }
    }
}
