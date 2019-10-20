using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables.Rows
{
    public class AssemblyDefinitionRowTest
    {
        [Fact]
        public void ReadRow_SmallBlob_SmallString()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream = peImage.DotNetDirectory.Metadata.GetStream<TablesStream>();

            var assemblyTable = tablesStream.GetTable<AssemblyDefinitionRow>();
            Assert.Single(assemblyTable);
            Assert.Equal(
                new AssemblyDefinitionRow(
                    (AssemblyHashAlgorithm) 0x00008004,
                    0x0001,
                    0x0000,
                    0x0000,
                    0x0000,
                    0x00000000,
                    0x0000,
                    0x0013,
                    0x0000),
                assemblyTable[0]);
        }
    }
}