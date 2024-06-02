using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables
{
    public class MemberReferenceRowTest
    {
        [Fact]
        public void ReadRow_SmallMemberRefParent_SmallString_SmallBlob()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream = peImage.DotNetDirectory!.Metadata!.GetStream<TablesStream>();

            var memberRefTable = tablesStream.GetTable<MemberReferenceRow>();
            Assert.Equal(12, memberRefTable.Count);
            Assert.Equal(
                new MemberReferenceRow(
                    0x0009,
                    0x0195,
                    0x0001),
                memberRefTable[0]);
            Assert.Equal(
                new MemberReferenceRow(
                    0x0061,
                    0x0195,
                    0x0006),
                memberRefTable[memberRefTable.Count - 1]);
        }

        [Fact]
        public void WriteRow_SmallMemberRefParent_SmallString_SmallBlob()
        {
            RowTestUtils.AssertWriteThenReadIsSame(
                new MemberReferenceRow(
                    0x0009,
                    0x0195,
                    0x0001),
                MemberReferenceRow.FromReader);
        }

        [Fact]
        public void RowEnumerationTest()
        {
            var rawRow = new uint[] {0x0009, 0x0195, 0x0001};
            var row = new MemberReferenceRow(rawRow[0], rawRow[1], rawRow[2]);

            RowTestUtils.VerifyRowColumnEnumeration(rawRow, row);
        }
    }
}
