using System;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Builder.Metadata;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder
{
    public class ManagedPEImageBuilderTest
    {
        [Fact]
        public void ExecutableImportDirectoryShouldContainMsCoreeCorExeMain()
        {
            using var stream = new MemoryStream();
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            module.Write(stream);

            var image = PEImage.FromStream(stream);
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
            var module = ModuleDefinition.FromBytes(Properties.Resources.ForwarderLibrary, TestReaderParameters);
            module.Write(stream);

            var image = PEImage.FromStream(stream);
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
            var newModule = ModuleDefinition.FromImage(result, TestReaderParameters);
            Assert.Equal(module.Name, newModule.Name);
        }

        [Fact]
        public void ConstructPEImageFromNewModuleWithPreservation()
        {
            var module = new ModuleDefinition("Module");
            var result = module.ToPEImage(new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveAll));
            var newModule = ModuleDefinition.FromImage(result, TestReaderParameters);
            Assert.Equal(module.Name, newModule.Name);
        }

        [Fact]
        public void ConstructPEImageFromExistingModuleWithNoPreservation()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var result = module.ToPEImage();
            var newModule = ModuleDefinition.FromImage(result, TestReaderParameters);
            Assert.Equal(module.Name, newModule.Name);
        }

        [Fact]
        public void ConstructPEImageFromExistingModuleWithPreservation()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var result = module.ToPEImage(new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveAll));
            var newModule = ModuleDefinition.FromImage(result, TestReaderParameters);
            Assert.Equal(module.Name, newModule.Name);
        }

        [Fact]
        public void ConstructPEImageFromNewModuleWithNoMemberSignaturePreservation()
        {
            // Prepare module.
            var module = new ModuleDefinition("Module");
            var factory = module.CorLibTypeFactory;
            var method = new MethodDefinition(
                "Foo",
                MethodAttributes.Public | MethodAttributes.Static,
                MethodSignature.CreateStatic(factory.Void)
            );
            module.GetOrCreateModuleType().Methods.Add(method);
            module.ManagedEntryPoint = method;

            // Introduce inconsistency in method metadata.
            method.IsStatic = false;

            // Verify default behavior stops building.
            Assert.ThrowsAny<AggregateException>(() => module.ToPEImage(new ManagedPEImageBuilder()));

            // Disabling metadata verification should make the build succeed.
            var image = module.ToPEImage(new ManagedPEImageBuilder(MetadataBuilderFlags.NoMemberSignatureVerification));

            var newModule = ModuleDefinition.FromImage(image, TestReaderParameters);
            var newMethod = newModule.GetModuleType()!.Methods.First(m => m.Name == "Foo");
            Assert.Equal(method.IsStatic, newMethod.IsStatic);
            Assert.Equal(method.Signature!.HasThis, newMethod.Signature!.HasThis);
        }

        [Fact]
        public void PreserveUnknownStreams()
        {
            // Prepare a PE image with an extra unconventional stream.
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters.PEReaderParameters);
            byte[] data = { 1, 2, 3, 4 };
            image.DotNetDirectory!.Metadata!.Streams.Add(new CustomMetadataStream("#Custom", data));

            // Load and rebuild.
            var module = ModuleDefinition.FromImage(image, TestReaderParameters);
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
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters.PEReaderParameters);
            var streams = image.DotNetDirectory!.Metadata!.Streams;
            for (int i = 0; i < streams.Count / 2; i++)
                (streams[i], streams[streams.Count - i - 1]) = (streams[streams.Count - i - 1], streams[i]);

            // Load and rebuild.
            var module = ModuleDefinition.FromImage(image, TestReaderParameters);
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
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters.PEReaderParameters);
            var streams = image.DotNetDirectory!.Metadata!.Streams;

            for (int i = 0; i < streams.Count / 2; i++)
                (streams[i], streams[streams.Count - i - 1]) = (streams[streams.Count - i - 1], streams[i]);

            byte[] data = { 1, 2, 3, 4 };
            image.DotNetDirectory!.Metadata!.Streams.Insert(streams.Count / 2,
                new CustomMetadataStream("#Custom", data));

            // Load and rebuild.
            var module = ModuleDefinition.FromImage(image, TestReaderParameters);
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

        [Fact]
        public void BuildingImageShouldConsiderJTDStreamAndUseLargeColumns()
        {
            var moduleDefinition = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_JTDStream, TestReaderParameters);
            var metadata = moduleDefinition.DotNetDirectory!.Metadata!;

            Assert.True(metadata.IsEncMetadata);
            Assert.True(metadata.GetStream<TablesStream>().ForceLargeColumns);

            var builder = new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveAll | MetadataBuilderFlags.ForceEncMetadata);
            var rebuiltImage = moduleDefinition.ToPEImage(builder);
            var rebuiltMetadata = rebuiltImage.DotNetDirectory!.Metadata!;

            Assert.True(rebuiltMetadata.IsEncMetadata);
            Assert.True(rebuiltMetadata.GetStream<TablesStream>().ForceLargeColumns);
        }
    }
}
