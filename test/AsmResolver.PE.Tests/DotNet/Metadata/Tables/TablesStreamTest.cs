using System.IO;
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

            using var tempStream = new MemoryStream();
            tablesStream.Write(new BinaryStreamWriter(tempStream));

            var context = new MetadataReaderContext(VirtualAddressFactory.Instance);
            var newTablesStream = new SerializedTableStream(context, tablesStream.Name, tempStream.ToArray());

            Assert.Equal(tablesStream.Reserved, newTablesStream.Reserved);
            Assert.Equal(tablesStream.MajorVersion, newTablesStream.MajorVersion);
            Assert.Equal(tablesStream.MinorVersion, newTablesStream.MinorVersion);
            Assert.Equal(tablesStream.Flags, newTablesStream.Flags);
            Assert.Equal(tablesStream.Log2LargestRid, newTablesStream.Log2LargestRid);
            Assert.Equal(tablesStream.ExtraData, newTablesStream.ExtraData);

            for (TableIndex i = 0; i <= TableIndex.GenericParamConstraint; i++)
            {
                var oldTable = tablesStream.GetTable(i);
                var newTable = newTablesStream.GetTable(i);

                Assert.Equal(oldTable.Count, newTable.Count);
                for (int j = 0; j < oldTable.Count; j++)
                    Assert.Equal(oldTable[j], newTable[j]);
            }
        }
    }
}
