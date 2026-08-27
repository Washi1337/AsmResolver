using System.IO;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.Builder;
using AsmResolver.PE.Debug;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.PE.Tests.Debug
{
    public class DebugDataEntryTest(TemporaryDirectoryFixture fixture) : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly TemporaryDirectoryFixture _fixture = fixture;

        private static PEImage RebuildAndReloadManagedPE(PEImage image)
        {
            // Build.
            using var tempStream = new MemoryStream();
            var builder = new ManagedPEFileBuilder();
            var newPeFile = builder.CreateFile(image);
            newPeFile.Write(new BinaryStreamWriter(tempStream));

            // Reload.
            var newImage = PEImage.FromBytes(tempStream.ToArray(), TestReaderParameters);
            return newImage;
        }

        [Fact]
        public void ReadEntries()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll, TestReaderParameters);

            Assert.Equal(new[]
            {
                DebugDataType.CodeView,
                DebugDataType.VcFeature
            }, image.DebugData.Select(d => d.Contents!.Type));
        }

        [Fact]
        public void PersistentEntries()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var newImage = RebuildAndReloadManagedPE(image);

            Assert.Equal(
                image.DebugData
                    .Where(e => e.Contents != null)
                    .Select(e => e.Contents!.Type),
                newImage.DebugData
                    .Where(e => e.Contents != null)
                    .Select(e => e.Contents!.Type));
        }

        [Fact]
        public void PersistentUtf8Characters()
        {
            // https://github.com/Washi1337/AsmResolver/issues/772

            const string expected = "/tmp/HelloWorld/obj-路径/HelloWorld.pdb";

            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_Utf8DebugPath, TestReaderParameters);
            var rsds = Assert.IsType<RsdsDataSegment>(image.DebugData[0].Contents, exactMatch: false);
            Assert.Equal(expected, rsds.Path);

            var newImage = RebuildAndReloadManagedPE(image);
            var newRsds = Assert.IsType<RsdsDataSegment>(newImage.DebugData[0].Contents, exactMatch: false);
            Assert.Equal(expected, newRsds.Path);

            _fixture.GetRunner<FrameworkPERunner>().RebuildAndRun(
                image.ToPEFile(new ManagedPEFileBuilder()),
                "HelloWorld.exe",
                "UNICODE_RSDS_FIXTURE_OK\n"
            );
        }

        [Fact]
        public void PersistentPortablePdbEntries()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);

            image.DebugData.Clear();
            image.DebugData.Add(new DebugDataEntry(
                new CustomDebugDataSegment(DebugDataType.EmbeddedPortablePdb, new DataSegment(new byte[] {1, 2, 3, 4}))));
            image.DebugData.Add(new DebugDataEntry(
                new CustomDebugDataSegment(DebugDataType.PdbChecksum, new DataSegment(new byte[] {5, 6, 7, 8}))));
            image.DebugData.Add(new DebugDataEntry(new EmptyDebugDataSegment(DebugDataType.Repro)));

            var newImage = RebuildAndReloadManagedPE(image);

            Assert.Equal(new[]
            {
                DebugDataType.EmbeddedPortablePdb,
                DebugDataType.PdbChecksum,
                DebugDataType.Repro
            }, newImage.DebugData.Select(d => d.Contents!.Type));
        }
    }
}
