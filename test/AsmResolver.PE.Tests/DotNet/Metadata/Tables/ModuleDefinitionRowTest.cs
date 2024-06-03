using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables
{
    public class ModuleDefinitionRowTest
    {
        [Fact]
        public void ReadRow_SmallString_SmallGuid()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream = peImage.DotNetDirectory!.Metadata!.GetStream<TablesStream>();

            var moduleTable = tablesStream.GetTable<ModuleDefinitionRow>();
            Assert.Single(moduleTable);
            Assert.Equal(
                new ModuleDefinitionRow(
                    0x0000,
                    0x0146,
                    0x0001,
                    0x0000, 0x0000),
                moduleTable[0]);
        }

        [Fact]
        public void WriteRow_SmallString_SmallGuid()
        {
            RowTestUtils.AssertWriteThenReadIsSame(new ModuleDefinitionRow(
                0x0000,
                0x0146,
                0x0001,
                0x0000, 0x0000),
                ModuleDefinitionRow.FromReader);
        }

        [Fact]
        public void RowEnumerationTest()
        {
            var rawRow = new uint[]
            {
                0x0000, 0x0146, 0x0001, 0x0000, 0x0000
            };
            var row = new ModuleDefinitionRow((ushort) rawRow[0], rawRow[1], rawRow[2],
                rawRow[3], rawRow[4]);

            RowTestUtils.VerifyRowColumnEnumeration(rawRow, row);
        }
    }
}
