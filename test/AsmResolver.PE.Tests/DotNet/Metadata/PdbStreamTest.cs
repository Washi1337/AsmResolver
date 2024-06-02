using System.IO;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata
{
    public class PdbStreamTest
    {
        private static IMetadata GetMetadata(bool rebuild)
        {
            var metadata = PE.DotNet.Metadata.Metadata.FromBytes(Properties.Resources.TheAnswerPortablePdb);
            if (rebuild)
            {
                using var stream = new MemoryStream();
                metadata.Write(new BinaryStreamWriter(stream));
                metadata = PE.DotNet.Metadata.Metadata.FromBytes(stream.ToArray());
            }

            return metadata;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Id(bool rebuild)
        {
            var metadata = GetMetadata(rebuild);
            Assert.Equal(new byte[]
            {
                0x95, 0x26, 0xB5, 0xAC, 0xA7, 0xB, 0xB1, 0x4D, 0x9B, 0xF3,
                0xCD, 0x31, 0x73, 0xB, 0xE9, 0x64, 0xBE, 0xFE, 0x11, 0xFC
            }, metadata.GetStream<PdbStream>().Id);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TypeSystemRowCounts(bool rebuild)
        {
            var metadata = GetMetadata(rebuild);
            var pdbStream = metadata.GetStream<PdbStream>();
            var tablesStream = metadata.GetStream<TablesStream>();

            Assert.Equal(new uint[]
            {
                1, 17, 2, 0, 0, 0, 5, 0, 3, 0, 16, 0, 12, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 2, 0, 0, 0, 1,
                0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            }, pdbStream.TypeSystemRowCounts);

            Assert.True(tablesStream.HasExternalRowCounts);
            Assert.Equal(
                tablesStream.ExternalRowCounts.Take((int) TableIndex.MaxTypeSystemTableIndex),
                pdbStream.TypeSystemRowCounts.Take((int) TableIndex.MaxTypeSystemTableIndex));
        }
    }
}
