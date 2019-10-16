using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables.Rows
{
    public class ModuleDefinitionRowTest
    {
        [Fact]
        public void ReadRow_SmallString_SmallGuid()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream = peImage.DotNetDirectory.Metadata.GetStream<TablesStream>();

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

    }
}