using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables.Rows
{
    public class MethodDefinitionRowTest
    {
        [Fact]
        public void ReadRow_SmallString_SmallBlob_SmallParam()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream = peImage.DotNetDirectory.Metadata.GetStream<TablesStream>();

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
        
    }
}