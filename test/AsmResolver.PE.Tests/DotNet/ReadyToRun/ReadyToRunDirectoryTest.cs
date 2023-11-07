using System.Collections.Generic;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.ReadyToRun;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.ReadyToRun
{
    public class ReadyToRunDirectoryTest
    {
        private static T GetSection<T>(IPEImage image, bool rebuild)
            where T : class, IReadyToRunSection
        {
            var serializedImage = (SerializedPEImage) image;

            var directory = Assert.IsAssignableFrom<ReadyToRunDirectory>(image.DotNetDirectory!.ManagedNativeHeader);
            var section = directory.GetSection<T>();

            if (rebuild)
            {
                section.UpdateOffsets(new RelocationParameters(0, 0));

                var reader = new BinaryStreamReader(section.WriteIntoArray());
                var context = serializedImage.ReaderContext;
                section = (T) context.Parameters.ReadyToRunSectionReader.ReadSection(context, section.Type, ref reader);
            }

            return section;
        }

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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CompilerIdentifierSection(bool rebuild)
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_ReadyToRun);
            var section = GetSection<CompilerIdentifierSection>(image, rebuild);

            Assert.Equal("Crossgen2 6.0.2223.42425", section.Identifier);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ImportSections(bool rebuild)
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_ReadyToRun);
            var section = GetSection<ImportSectionsSection>(image, rebuild);

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

        [Theory]
        [InlineData(false)]
        // TODO: [InlineData(true)]
        public void ImportSectionSlots(bool rebuild)
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_ReadyToRun);
            var section = GetSection<ImportSectionsSection>(image, rebuild);

            Assert.Equal(new[]
            {
                (0x00000000u, 0x0000A0D4u, ReadyToRunFixupKind.CheckInstructionSetSupport),
                (0x00000000u, 0x0000A0DBu, ReadyToRunFixupKind.Helper),
                (0x00000000u, 0x0000A0DDu, ReadyToRunFixupKind.Helper),
                (0x00000000u, 0x0000A0DFu, ReadyToRunFixupKind.Helper),
                (0x00000000u, 0x0000A0E2u, ReadyToRunFixupKind.Helper),
            }, ReadTestData(section.Sections[1]));

            Assert.Equal(new[]
            {
                (0x0009594u, 0x0000A0D9u, ReadyToRunFixupKind.MethodEntryRefToken),
            }, ReadTestData(section.Sections[3]));

            Assert.Equal(new[]
            {
                (0x00000000u, 0x0000A0E5u, ReadyToRunFixupKind.StringHandle),
            }, ReadTestData(section.Sections[5]));

            return;

            IEnumerable<(uint, uint, ReadyToRunFixupKind)> ReadTestData(ImportSection import)
            {
                for (int i = 0; i < import.Slots.Count; i++)
                {
                    yield return (
                        import.Slots[i].Rva,
                        import.Signatures[i].Rva,
                        (ReadyToRunFixupKind) import.Signatures[i].CreateReader().ReadByte()
                    );
                }
            }
        }

        [Fact]
        public void X64RuntimeFunctions()
        {
            var image = PEImage.FromBytes(Properties.Resources.ReadyToRunTest);
            var header = Assert.IsAssignableFrom<ReadyToRunDirectory>(image.DotNetDirectory!.ManagedNativeHeader);
            var section = header.GetSection<RuntimeFunctionsSection>();

            Assert.Equal(new[]
            {
                (0x00001840u, 0x00001870u),
                (0x00001870u, 0x0000188au),
                (0x00001890u, 0x000018cdu),
                (0x000018CDu, 0x000018f2u),
                (0x00001900u, 0x0000191au),
            }, section.GetFunctions().Select(x => (x.Begin.Rva, x.End.Rva)));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void MethodDefEntryPoints(bool rebuild)
        {
            var image = PEImage.FromBytes(Properties.Resources.ReadyToRunTest);
            var section = GetSection<MethodEntryPointsSection>(image, rebuild);

            Assert.Equal(
                new uint?[] {0u, null, 1u, 2u, 4u},
                section.EntryPoints.Select(x => x?.RuntimeFunctionIndex)
            );
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void MethodDefEntryPointFixups(bool rebuild)
        {
            var image = PEImage.FromBytes(Properties.Resources.ReadyToRunTest);
            var section = GetSection<MethodEntryPointsSection>(image, rebuild);

            Assert.Equal(new[]
            {
                (5u, 0u),
                (5u, 4u),
            }, section.EntryPoints[0]!.Fixups.Select(x => (x.ImportIndex, x.SlotIndex)));
        }
    }
}
