using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables.Rows
{
    public class AssemblyReferenceRowTest
    {
        [Fact]
        public void ReadRow_SmallBlob_SmallString()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream = peImage.DotNetDirectory.Metadata.GetStream<TablesStream>();

            var assemblyTable = tablesStream.GetTable<AssemblyReferenceRow>();
            Assert.Single(assemblyTable);
            Assert.Equal(
                new AssemblyReferenceRow(
                    0x0004,
                    0x0000,
                    0x0000,
                    0x0000,
                    0x00000000,
                    0x001A,
                    0x000A,
                    0x0000,
                    0x0000), 
                assemblyTable[0]);
        }
    }
}