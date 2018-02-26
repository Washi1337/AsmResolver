using System.Linq;
using AsmResolver.Net;
using Xunit;

namespace AsmResolver.Tests.Net
{
    public class NetAssemblyFactoryTest
    {
        [Fact]
        public void ImportDirectory()
        {
            var assembly = NetAssemblyFactory.CreateAssembly("SomeAssembly", true);
            Assert.Contains("mscoree.dll", assembly.ImportDirectory.ModuleImports.Select(x => x.Name));
            Assert.Contains("_CorDllMain", assembly.ImportDirectory.ModuleImports.First(x => x.Name == "mscoree.dll").SymbolImports.Select(x => x.HintName.Name));

            assembly = NetAssemblyFactory.CreateAssembly("SomeAssembly", false);
            Assert.Contains("mscoree.dll", assembly.ImportDirectory.ModuleImports.Select(x => x.Name));
            Assert.Contains("_CorExeMain", assembly.ImportDirectory.ModuleImports.First(x => x.Name == "mscoree.dll").SymbolImports.Select(x => x.HintName.Name));
        }

        [Fact]
        public void MetadataHeaders()
        {
            var assembly = NetAssemblyFactory.CreateAssembly("SomeAssembly", true);

            Assert.NotNull(assembly.NetDirectory);
            Assert.NotNull(assembly.NetDirectory.MetadataHeader);
            Assert.Null(assembly.NetDirectory.MetadataHeader.Image);
        }

        [Fact]
        public void MetadataImage()
        {
            var assembly = NetAssemblyFactory.CreateAssembly("SomeAssembly", true);

            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
            Assert.NotNull(image.Assembly);
            Assert.Equal("SomeAssembly", image.Assembly.Name);
        }
    }
}
