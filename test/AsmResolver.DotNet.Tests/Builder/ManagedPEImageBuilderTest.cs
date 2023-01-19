using System.IO;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata;
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

        [Fact]
        public void PreserveUnknownStreams()
        {
            // Prepare a PE image with an extra unconventional stream.
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);
            byte[] data = { 1, 2, 3, 4 };
            image.DotNetDirectory!.Metadata!.Streams.Add(new CustomMetadataStream("#Custom", data));

            // Load and rebuild.
            var module = ModuleDefinition.FromImage(image);
            var newImage = module.ToPEImage(new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveUnknownStreams));

            // Verify unconventional stream is still present.
            var newStream = Assert.IsAssignableFrom<CustomMetadataStream>(
                newImage.DotNetDirectory!.Metadata!.GetStream("#Custom"));
            Assert.Equal(data, Assert.IsAssignableFrom<IReadableSegment>(newStream.Contents).ToArray());
        }

        [Fact]
        public void PreserveStreamOrder()
        {
            // Prepare a PE image with an unconventional stream order.
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var streams = image.DotNetDirectory!.Metadata!.Streams;
            for (int i = 0; i < streams.Count / 2; i++)
                (streams[i], streams[streams.Count - i - 1]) = (streams[streams.Count - i - 1], streams[i]);

            // Load and rebuild.
            var module = ModuleDefinition.FromImage(image);
            var newImage = module.ToPEImage(new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveStreamOrder));

            // Verify order is still the same.
            Assert.Equal(
                streams.Select(x => x.Name),
                newImage.DotNetDirectory!.Metadata!.Streams.Select(x => x.Name));
        }

        [Fact]
        public void PreserveUnknownStreamsAndStreamOrder()
        {
            // Prepare a PE image with an unconventional stream order and custom stream.
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var streams = image.DotNetDirectory!.Metadata!.Streams;

            for (int i = 0; i < streams.Count / 2; i++)
                (streams[i], streams[streams.Count - i - 1]) = (streams[streams.Count - i - 1], streams[i]);

            byte[] data = { 1, 2, 3, 4 };
            image.DotNetDirectory!.Metadata!.Streams.Insert(streams.Count / 2,
                new CustomMetadataStream("#Custom", data));

            // Load and rebuild.
            var module = ModuleDefinition.FromImage(image);
            var newImage = module.ToPEImage(new ManagedPEImageBuilder(
                MetadataBuilderFlags.PreserveStreamOrder | MetadataBuilderFlags.PreserveUnknownStreams));

            // Verify order is still the same.
            Assert.Equal(
                streams.Select(x => x.Name),
                newImage.DotNetDirectory!.Metadata!.Streams.Select(x => x.Name));

            // Verify unconventional stream is still present.
            var newStream = Assert.IsAssignableFrom<CustomMetadataStream>(
                newImage.DotNetDirectory!.Metadata!.GetStream("#Custom"));
            Assert.Equal(data, Assert.IsAssignableFrom<IReadableSegment>(newStream.Contents).ToArray());
        }
    }
}
