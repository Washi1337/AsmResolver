using System.IO;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.Builder;
using AsmResolver.PE.Debug;
using Xunit;

namespace AsmResolver.PE.Tests.Debug
{
    public class DebugDataEntryTest
    {
        private static PEImage RebuildAndReloadManagedPE(PEImage image)
        {
            // Build.
            using var tempStream = new MemoryStream();
            var builder = new ManagedPEFileBuilder();
            var newPeFile = builder.CreateFile(image);
            newPeFile.Write(new BinaryStreamWriter(tempStream));

            // Reload.
            var newImage = PEImage.FromBytes(tempStream.ToArray());
            return newImage;
        }

        [Fact]
        public void ReadEntries()
        {
            var image = PEImage.FromBytes(Properties.Resources.SimpleDll);

            Assert.Equal(new[]
            {
                DebugDataType.CodeView,
                DebugDataType.VcFeature
            }, image.DebugData.Select(d => d.Contents!.Type));
        }

        [Fact]
        public void PersistentEntries()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var newImage = RebuildAndReloadManagedPE(image);

            Assert.Equal(
                image.DebugData
                    .Where(e => e.Contents != null)
                    .Select(e => e.Contents!.Type),
                newImage.DebugData
                    .Where(e => e.Contents != null)
                    .Select(e => e.Contents!.Type));
        }
    }
}
