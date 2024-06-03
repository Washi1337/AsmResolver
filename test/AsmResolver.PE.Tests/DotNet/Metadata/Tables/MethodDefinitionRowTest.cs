using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables
{
    public class MethodDefinitionRowTest
    {
        [Fact]
        public void ReadRow_SmallString_SmallBlob_SmallParam()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream = peImage.DotNetDirectory!.Metadata!.GetStream<TablesStream>();

            var methodTable = tablesStream.GetTable<MethodDefinitionRow>();
            Assert.Equal(2, methodTable.Count);
            Assert.Equal(
                new MethodDefinitionRow(
                    new VirtualAddress( 0x00002050),
                    0x0000,
                    (MethodAttributes) 0x0091,
                    0x017E,
                    0x0023,
                    0x0001),
                methodTable[0]);
            Assert.Equal(
                new MethodDefinitionRow(
                    new VirtualAddress( 0x0000205C),
                    0x0000,
                    (MethodAttributes) 0x1886,
                    0x0195,
                    0x0006,
                    0x0002),
                methodTable[1]);
        }

        [Fact]
        public void WriteRow_SmallStrinmg_SmallBlob_SmallParam()
        {
            RowTestUtils.AssertWriteThenReadIsSame(new MethodDefinitionRow(
                    new VirtualAddress( 0x00002050),
                    0x0000,
                    (MethodAttributes) 0x0091,
                    0x017E,
                    0x0023,
                    0x0001),
                MethodDefinitionRow.FromReader);
        }

        [Fact]
        public void RowEnumerationTest()
        {
            var rawRow = new uint[] {0x00002050, 0x0000, 0x0091, 0x017E, 0x0023, 0x0001};
            var row = new MethodDefinitionRow(
                new VirtualAddress(rawRow[0]),
                (MethodImplAttributes) rawRow[1],
                (MethodAttributes) rawRow[2],
                rawRow[3],
                rawRow[4],
                rawRow[5]);

            RowTestUtils.VerifyRowColumnEnumeration(rawRow, row);
        }
    }
}
