using System.IO;
using System.Linq;
using AsmResolver.DotNet.Builder;
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
                && relocation.Location.CreateReader().ReadUInt32() == image.ImageBase + symbol.AddressTableEntry!.Rva);
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
                && relocation.Location.CreateReader().ReadUInt32() == image.ImageBase + symbol.AddressTableEntry!.Rva);
        }

        [Fact]
        public void ConstructPEImageFromNewModuleWithNoPreservation()
        {
            var module = new ModuleDefinition("Module");
            var result = module.ToPEImage();
            var newModule = ModuleDefinition.FromImage(result);
            Assert.Equal(module.Name, newModule.Name);
        }

        [Fact]
        public void ConstructPEImageFromNewModuleWithPreservation()
        {
            var module = new ModuleDefinition("Module");
            var result = module.ToPEImage(new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveAll));
            var newModule = ModuleDefinition.FromImage(result);
            Assert.Equal(module.Name, newModule.Name);
        }

        [Fact]
        public void ConstructPEImageFromExistingModuleWithNoPreservation()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var result = module.ToPEImage();
            var newModule = ModuleDefinition.FromImage(result);
            Assert.Equal(module.Name, newModule.Name);
        }

        [Fact]
        public void ConstructPEImageFromExistingModuleWithPreservation()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var result = module.ToPEImage(new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveAll));
            var newModule = ModuleDefinition.FromImage(result);
            Assert.Equal(module.Name, newModule.Name);
        }
    }
}
