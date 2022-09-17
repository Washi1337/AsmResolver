using System.Linq;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Tables
{
    public class MetadataRangeTest
    {
        [Fact]
        public void ContinuousRangeEmpty()
        {
            var range = new MetadataRange(TableIndex.Method, 3, 3);
            Assert.Equal(0, range.Count);
            Assert.Empty(range);
        }

        [Fact]
        public void ContinuousRangeSingleItem()
        {
            var range = new MetadataRange(TableIndex.Method, 3, 4);
            Assert.Equal(1, range.Count);
            Assert.Single(range);
            Assert.Equal(new MetadataToken(TableIndex.Method, 3), range.First());
        }

        [Fact]
        public void ContinuousRangeMultipleItems()
        {
            var range = new MetadataRange(TableIndex.Method, 3, 10);
            Assert.Equal(7, range.Count);
            Assert.Equal(new[]
            {
                new MetadataToken(TableIndex.Method, 3),
                new MetadataToken(TableIndex.Method, 4),
                new MetadataToken(TableIndex.Method, 5),
                new MetadataToken(TableIndex.Method, 6),
                new MetadataToken(TableIndex.Method, 7),
                new MetadataToken(TableIndex.Method, 8),
                new MetadataToken(TableIndex.Method, 9)
            }, range);
        }

        [Fact]
        public void RedirectedRangeEmpty()
        {
            var stream = new TablesStream();
            var redirectTable = stream.GetTable<MethodPointerRow>();

            var range = new MetadataRange(redirectTable, TableIndex.Method, 3, 3);
            Assert.Equal(0, range.Count);
            Assert.Empty(range);
        }

        [Fact]
        public void RedirectedRangeSingleItem()
        {
            var stream = new TablesStream();
            var redirectTable = stream.GetTable<MethodPointerRow>();
            redirectTable.Add(new MethodPointerRow(1));
            redirectTable.Add(new MethodPointerRow(2));
            redirectTable.Add(new MethodPointerRow(5));
            redirectTable.Add(new MethodPointerRow(4));
            redirectTable.Add(new MethodPointerRow(3));

            var range = new MetadataRange(redirectTable, TableIndex.Method, 3, 4);
            Assert.Equal(1, range.Count);
            Assert.Single(range);
            Assert.Equal(new MetadataToken(TableIndex.Method, 5), range.First());
        }

        [Fact]
        public void RedirectedRangeMultipleItems()
        {
            var stream = new TablesStream();
            var redirectTable = stream.GetTable<MethodPointerRow>();
            redirectTable.Add(new MethodPointerRow(1));
            redirectTable.Add(new MethodPointerRow(2));
            redirectTable.Add(new MethodPointerRow(5));
            redirectTable.Add(new MethodPointerRow(4));
            redirectTable.Add(new MethodPointerRow(3));
            redirectTable.Add(new MethodPointerRow(9));
            redirectTable.Add(new MethodPointerRow(8));
            redirectTable.Add(new MethodPointerRow(10));

            var range = new MetadataRange(redirectTable, TableIndex.Method, 3, 8);
            Assert.Equal(5, range.Count);
            Assert.Equal(new[]
            {
                new MetadataToken(TableIndex.Method, 5),
                new MetadataToken(TableIndex.Method, 4),
                new MetadataToken(TableIndex.Method, 3),
                new MetadataToken(TableIndex.Method, 9),
                new MetadataToken(TableIndex.Method, 8)
            }, range);
        }

    }
}
