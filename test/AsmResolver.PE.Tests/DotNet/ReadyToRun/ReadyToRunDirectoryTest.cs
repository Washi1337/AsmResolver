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
    }
}
