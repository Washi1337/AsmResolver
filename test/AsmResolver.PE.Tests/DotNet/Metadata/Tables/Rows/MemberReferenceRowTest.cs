using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables.Rows
{
    public class MemberReferenceRowTest
    {
        [Fact]
        public void ReadRow_SmallMemberRefParent_SmallString_SmallBlob()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var tablesStream = peImage.DotNetDirectory.Metadata.GetStream<TablesStream>();

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
    }
}