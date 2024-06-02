using System.IO;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.File;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables
{
    public class TablesStreamTest
    {
        [Fact]
        public void DetectNoExtraData()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream = peImage.DotNetDirectory!.Metadata!.GetStream<TablesStream>();

            Assert.False(tablesStream.HasExtraData);
        }

        [Fact]
        public void DetectExtraData()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld_TablesStream_ExtraData);
            var tablesStream = peImage.DotNetDirectory!.Metadata!.GetStream<TablesStream>();

            Assert.True(tablesStream.HasExtraData);
            Assert.Equal(12345678u, tablesStream.ExtraData);
        }

        [Fact]
        public void PreserveTableStreamNoChange()
        {
            var peFile = PEFile.FromBytes(Properties.Resources.HelloWorld);
            var peImage = PEImage.FromFile(peFile);
            var tablesStream = peImage.DotNetDirectory!.Metadata!.GetStream<TablesStream>();

            AssertEquivalentAfterRebuild(tablesStream);
        }

        [Fact]
        public void SmallExternalIndicesShouldHaveSmallIndicesInTablesStream()
        {
            var pdbMetadata = PE.DotNet.Metadata.Metadata.FromBytes(Properties.Resources.TheAnswerPortablePdb);
            var stream = pdbMetadata.GetStream<TablesStream>();
            Assert.Equal(IndexSize.Short, stream.GetIndexEncoder(CodedIndex.HasCustomAttribute).IndexSize);
        }

        [Fact]
        public void LargeExternalIndicesShouldHaveLargeIndicesInTablesStream()
        {
            var pdbMetadata = PE.DotNet.Metadata.Metadata.FromBytes(Properties.Resources.LargeIndicesPdb);
            var stream = pdbMetadata.GetStream<TablesStream>();
            Assert.Equal(IndexSize.Long, stream.GetIndexEncoder(CodedIndex.HasCustomAttribute).IndexSize);
        }

        [Fact]
        public void PreservePdbTableStreamWithSmallExternalIndicesNoChange()
        {
            var pdbMetadata = PE.DotNet.Metadata.Metadata.FromBytes(Properties.Resources.TheAnswerPortablePdb);
            AssertEquivalentAfterRebuild(pdbMetadata.GetStream<TablesStream>());
        }

        [Fact]
        public void PreservePdbTableStreamWithLargeExternalIndicesNoChange()
        {
            var pdbMetadata = PE.DotNet.Metadata.Metadata.FromBytes(Properties.Resources.LargeIndicesPdb);
            AssertEquivalentAfterRebuild(pdbMetadata.GetStream<TablesStream>());
        }

        [Fact]
        public void GetImpliedTableRowCountFromNonPdbMetadataShouldGetLocalRowCount()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var stream = peImage.DotNetDirectory!.Metadata!.GetStream<TablesStream>();
            Assert.Equal((uint) stream.GetTable(TableIndex.TypeDef).Count, stream.GetTableRowCount(TableIndex.TypeDef));
        }

        [Fact]
        public void GetImpliedTableRowCountFromPdbMetadataShouldGetExternalRowCount()
        {
            var pdbMetadata =  PE.DotNet.Metadata.Metadata.FromBytes(Properties.Resources.TheAnswerPortablePdb);
            var stream = pdbMetadata.GetStream<TablesStream>();
            Assert.Equal(2u, stream.GetTableRowCount(TableIndex.TypeDef));
            Assert.Equal(0u ,(uint) stream.GetTable(TableIndex.TypeDef).Count);
        }

        private static void AssertEquivalentAfterRebuild(TablesStream tablesStream)
        {
            using var tempStream = new MemoryStream();
            tablesStream.Write(new BinaryStreamWriter(tempStream));

            var context = new MetadataReaderContext(VirtualAddressFactory.Instance);
            var newTablesStream = new SerializedTableStream(context, tablesStream.Name, tempStream.ToArray());

            var metadata = new PE.DotNet.Metadata.Metadata();
            if (tablesStream.HasExternalRowCounts)
            {
                var pdbStream = new PdbStream();
                pdbStream.UpdateRowCounts(tablesStream.ExternalRowCounts);
                metadata.Streams.Add(pdbStream);
            }
            newTablesStream.Initialize(metadata);

            Assert.Equal(tablesStream.Reserved, newTablesStream.Reserved);
            Assert.Equal(tablesStream.MajorVersion, newTablesStream.MajorVersion);
            Assert.Equal(tablesStream.MinorVersion, newTablesStream.MinorVersion);
            Assert.Equal(tablesStream.Flags, newTablesStream.Flags);
            Assert.Equal(tablesStream.Log2LargestRid, newTablesStream.Log2LargestRid);
            Assert.Equal(tablesStream.ExtraData, newTablesStream.ExtraData);

            Assert.All(Enumerable.Range(0, (int) TableIndex.Max), i =>
            {
                var tableIndex = (TableIndex) i;
                if (!tableIndex.IsValidTableIndex())
                    return;

                var oldTable = tablesStream.GetTable(tableIndex);
                var newTable = newTablesStream.GetTable(tableIndex);

                Assert.Equal(oldTable.IsSorted, newTable.IsSorted);
                Assert.Equal(oldTable.Count, newTable.Count);
                Assert.All(Enumerable.Range(0, oldTable.Count), j => Assert.Equal(oldTable[j], newTable[j]));
            });
        }
    }
}
