using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables
{
    public class ModuleDefinitionRowTest
    {
        [Fact]
        public void ReadRowSmallStringSmallGuid()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream =
                (TablesStream) peImage.DotNetDirectory.Metadata.GetStream(TablesStream.CompressedStreamName);

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