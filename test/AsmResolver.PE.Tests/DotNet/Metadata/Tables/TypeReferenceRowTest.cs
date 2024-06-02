using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables
{
    public class TypeReferenceRowTest
    {
        [Fact]
        public void ReadRow_SmallResolutionScope_SmallStrings()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream = peImage.DotNetDirectory!.Metadata!.GetStream<TablesStream>();

            var typeRefTable = tablesStream.GetTable<TypeReferenceRow>();
            Assert.Equal(13, typeRefTable.Count);
            Assert.Equal(
                new TypeReferenceRow(0x0006, 0x00D6, 0x01AE),
                typeRefTable[0]);
            Assert.Equal(
                new TypeReferenceRow(0x0006, 0x001E, 0x0177),
                typeRefTable[^1]);
        }

        [Fact]
        public void WriteRow_SmallResolutionScope_SmallStrings()
        {
            RowTestUtils.AssertWriteThenReadIsSame(
                new TypeReferenceRow(0x0006, 0x00D6, 0x01AE),
                TypeReferenceRow.FromReader);
        }

        [Fact]
        public void RowEnumerationTest()
        {
            var rawRow = new uint[] { 0x0006, 0x00D6, 0x01AE };
            var row = new TypeReferenceRow(rawRow[0], rawRow[1], rawRow[2]);

            RowTestUtils.VerifyRowColumnEnumeration(rawRow, row);
        }

    }
}
