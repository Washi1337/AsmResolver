using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables.Rows
{
    public class ParameterDefinitionRowTest
    {
        [Fact]
        public void ReadRow_SmallString()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream = peImage.DotNetDirectory.Metadata.GetStream<TablesStream>();

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
        
    }
}