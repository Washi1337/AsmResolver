using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables.Rows
{
    public class TypeReferenceRowTest
    {
        [Fact]
        public void ReadRowSmallStringSmallGuid()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream = peImage.DotNetDirectory.Metadata.GetStream<TablesStream>();

            var typeRefTable = tablesStream.GetTable<TypeReferenceRow>();
            Assert.Equal(13, typeRefTable.Count);
            Assert.Equal(
                new TypeReferenceRow(0x0006, 0x00D6, 0x01AE),
                typeRefTable[0]);
            Assert.Equal(
                new TypeReferenceRow(0x0006, 0x001E, 0x0177),
                typeRefTable[typeRefTable.Count - 1]);
        }
        
    }
}