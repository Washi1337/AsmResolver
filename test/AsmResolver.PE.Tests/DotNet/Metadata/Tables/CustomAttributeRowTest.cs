using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables
{
    public class CustomAttributeRowTest
    {
        [Fact]
        public void ReadRow_SmallHasCA_SmallCAType_SmallBlob()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream = peImage.DotNetDirectory!.Metadata!.GetStream<TablesStream>();

            var customAttributeTable = tablesStream.GetTable<CustomAttributeRow>();
            Assert.Equal(10, customAttributeTable.Count);
            Assert.Equal(
                new CustomAttributeRow(
                    0x002E,
                    0x000B,
                    0x0029),
                customAttributeTable[0]);
            Assert.Equal(
                new CustomAttributeRow(
                    0x002E,
                    0x0053,
                    0x00A2),
                customAttributeTable[customAttributeTable.Count - 1]);
        }

        [Fact]
        public void WriteRow_SmallHasCA_SmallCAType_SmallBlob()
        {
            RowTestUtils.AssertWriteThenReadIsSame(
                new CustomAttributeRow(
                    0x002E,
                    0x000B,
                    0x0029),
                CustomAttributeRow.FromReader);
        }

        [Fact]
        public void RowEnumerationTest()
        {
            var rawRow = new uint[] { 0x002E, 0x000B, 0x0029 };
            var row = new CustomAttributeRow(rawRow[0], rawRow[1], rawRow[2]);

            RowTestUtils.VerifyRowColumnEnumeration(rawRow, row);
        }
    }
}
