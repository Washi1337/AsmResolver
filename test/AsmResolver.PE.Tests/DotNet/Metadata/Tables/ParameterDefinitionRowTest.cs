using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables
{
    public class ParameterDefinitionRowTest
    {
        [Fact]
        public void ReadRow_SmallString()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream = peImage.DotNetDirectory!.Metadata!.GetStream<TablesStream>();

            var paramTable = tablesStream.GetTable<ParameterDefinitionRow>();
            Assert.Single(paramTable);
            Assert.Equal(
                new ParameterDefinitionRow(0x0000, 0x001, 0x01DD),
                paramTable[0]);
        }

        [Fact]
        public void WriteRow_SmallString()
        {
            RowTestUtils.AssertWriteThenReadIsSame(
                new ParameterDefinitionRow(0x0000, 0x001, 0x01DD),
                ParameterDefinitionRow.FromReader);
        }

        [Fact]
        public void RowEnumerationTest()
        {
            var rawRow = new uint[] { 0x0000, 0x001, 0x01DD };
            var row = new ParameterDefinitionRow((ParameterAttributes) rawRow[0], (ushort) rawRow[1], rawRow[2]);

            RowTestUtils.VerifyRowColumnEnumeration(rawRow, row);
        }
    }
}
