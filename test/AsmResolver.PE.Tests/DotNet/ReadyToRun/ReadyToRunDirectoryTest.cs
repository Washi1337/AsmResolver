using System.Linq;
using AsmResolver.PE.DotNet.ReadyToRun;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.ReadyToRun
{
    public class ReadyToRunDirectoryTest
    {
        [Fact]
        public void ReadBasicHeader()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_ReadyToRun);
            var header = Assert.IsAssignableFrom<ReadyToRunDirectory>(image.DotNetDirectory!.ManagedNativeHeader);

            Assert.Equal(5, header.MajorVersion);
            Assert.Equal(4, header.MinorVersion);
            Assert.Equal(ReadyToRunCoreHeaderAttributes.NonSharedPInvokeStubs, header.Attributes);
        }

        [Fact]
        public void ReadSections()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_ReadyToRun);
            var header = Assert.IsAssignableFrom<ReadyToRunDirectory>(image.DotNetDirectory!.ManagedNativeHeader);

            Assert.Equal(new[]
            {
                ReadyToRunSectionType.CompilerIdentifier,
                ReadyToRunSectionType.ImportSections,
                ReadyToRunSectionType.RuntimeFunctions,
                ReadyToRunSectionType.MethodDefEntryPoints,
                ReadyToRunSectionType.DebugInfo,
                ReadyToRunSectionType.DelayLoadMethodCallThunks,
                ReadyToRunSectionType.AvailableTypes,
                ReadyToRunSectionType.InstanceMethodEntryPoints,
                ReadyToRunSectionType.ManifestMetadata,
                ReadyToRunSectionType.InliningInfo2,
                ReadyToRunSectionType.ManifestAssemblyMvids,
            }, header.Sections.Select(x => x.Type));
        }

        [Fact]
        public void ReadCompilerIdentifierSection()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_ReadyToRun);
            var header = Assert.IsAssignableFrom<ReadyToRunDirectory>(image.DotNetDirectory!.ManagedNativeHeader);
            var section = header.GetSection<CompilerIdentifierSection>();

            Assert.Equal("Crossgen2 6.0.2223.42425", section.Identifier);
        }

        [Fact]
        public void ReadImportSections()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_ReadyToRun);
            var header = Assert.IsAssignableFrom<ReadyToRunDirectory>(image.DotNetDirectory!.ManagedNativeHeader);
            var section = header.GetSection<ImportSectionsSection>();

            Assert.Equal(new[]
            {
                ImportSectionType.StubDispatch,
                ImportSectionType.Unknown,
                ImportSectionType.Unknown,
                ImportSectionType.StubDispatch,
                ImportSectionType.Unknown,
                ImportSectionType.StringHandle
            }, section.Sections.Select(x => x.Type));
        }

        [Fact]
        public void ReadImportSectionSlots()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_ReadyToRun);
            var header = Assert.IsAssignableFrom<ReadyToRunDirectory>(image.DotNetDirectory!.ManagedNativeHeader);
            var section = header.GetSection<ImportSectionsSection>();

            Assert.Equal(new[]
            {
                (0x00000000u, 0x0000A0D4u),
                (0x00000000u, 0x0000A0DBu),
                (0x00000000u, 0x0000A0DDu),
                (0x00000000u, 0x0000A0DFu),
                (0x00000000u, 0x0000A0E2u),
            }, section.Sections[1].Slots.Select((x,i) => (x.Rva, section.Sections[1].Signatures[i].Rva)));

            Assert.Equal(new[]
            {
                (0x0009594u, 0x0000A0D9u),
            }, section.Sections[3].Slots.Select((x,i) => (x.Rva, section.Sections[3].Signatures[i].Rva)));

            Assert.Equal(new[]
            {
                (0x00000000u, 0x0000A0E5u),
            }, section.Sections[5].Slots.Select((x,i) => (x.Rva, section.Sections[5].Signatures[i].Rva)));
        }
    }
}
