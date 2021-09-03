using System.IO;
using System.Linq;
using AsmResolver.PE;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder
{
    public class ManagedPEImageBuilderTest
    {
        [Fact]
        public void ExecutableImportDirectoryShouldContainMsCoreeCorExeMain()
        {
            using var stream = new MemoryStream();
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            module.Write(stream);

            var image = PEImage.FromBytes(stream.ToArray());
            var symbol = image
                .Imports.FirstOrDefault(m => m.Name == "mscoree.dll")
                ?.Symbols.FirstOrDefault(m => m.Name == "_CorExeMain");

            Assert.NotNull(symbol);
            Assert.Contains(image.Relocations, relocation =>
                relocation.Location.CanRead
                && relocation.Location.CreateReader().ReadUInt32() == symbol.AddressTableEntry!.Rva);
        }

        [Fact]
        public void ExecutableImportDirectoryShouldContainMsCoreeCorDllMain()
        {
            using var stream = new MemoryStream();
            var module = ModuleDefinition.FromBytes(Properties.Resources.ForwarderLibrary);
            module.Write(stream);

            var image = PEImage.FromBytes(stream.ToArray());
            var symbol = image
                .Imports.FirstOrDefault(m => m.Name == "mscoree.dll")
                ?.Symbols.FirstOrDefault(m => m.Name == "_CorDllMain");

            Assert.NotNull(symbol);
            Assert.Contains(image.Relocations, relocation =>
                relocation.Location.CanRead
                && relocation.Location.CreateReader().ReadUInt32() == symbol.AddressTableEntry!.Rva);
        }
    }
}
