using AsmResolver.Net.Metadata;
using Xunit;

namespace AsmResolver.Tests.Net.Metadata
{
    public class SortedTableTest
    {
        [Fact]
        public void InsertIntoEmptyTable()
        {
            var sortedTable = new DummySortedTable();
            Assert.Empty(sortedTable);

            const uint newValue = 123;
            sortedTable.Add(new MetadataRow<uint> {Column1 = newValue});
            
            Assert.Single(sortedTable);
            Assert.Equal(newValue, sortedTable[0].Column1);
        }
        
        [Fact]
        public void InsertSmaller()
        {
            var sortedTable = new DummySortedTable {new MetadataRow<uint> {Column1 = 123}};
            Assert.Single(sortedTable);

            const uint newValue = 10;
            sortedTable.Add(new MetadataRow<uint> {Column1 = newValue});
            
            Assert.Equal(2, sortedTable.Count);
            Assert.Equal(newValue, sortedTable[0].Column1);
        }
        
        [Fact]
        public void InsertBigger()
        {
            var sortedTable = new DummySortedTable {new MetadataRow<uint> {Column1 = 123}};
            Assert.Single(sortedTable);

            const uint newValue = 200;
            sortedTable.Add(new MetadataRow<uint> {Column1 = newValue});
            
            Assert.Equal(2, sortedTable.Count);
            Assert.Equal(newValue, sortedTable[1].Column1);
        }

        [Fact]
        public void InsertIntoMiddleOfList()
        {
            var sortedTable = new DummySortedTable
            {
                new MetadataRow<uint> {Column1 = 10},
                new MetadataRow<uint> {Column1 = 20},
                new MetadataRow<uint> {Column1 = 30},
                new MetadataRow<uint> {Column1 = 40},
                new MetadataRow<uint> {Column1 = 50},
            };

            const uint newValue = 25;
            sortedTable.Add(new MetadataRow<uint> {Column1 = newValue});
            
            Assert.Equal(6, sortedTable.Count);
            Assert.Equal(newValue, sortedTable[2].Column1);
        }

        [Fact]
        public void InsertIntoBeginOfList()
        {
            var sortedTable = new DummySortedTable
            {
                new MetadataRow<uint> {Column1 = 10},
                new MetadataRow<uint> {Column1 = 20},
                new MetadataRow<uint> {Column1 = 30},
                new MetadataRow<uint> {Column1 = 40},
                new MetadataRow<uint> {Column1 = 50},
            };

            const uint newValue = 1;
            sortedTable.Add(new MetadataRow<uint> {Column1 = newValue});
            
            Assert.Equal(6, sortedTable.Count);
            Assert.Equal(newValue, sortedTable[0].Column1);
        }

        [Fact]
        public void InsertIntoEndOfList()
        {
            var sortedTable = new DummySortedTable
            {
                new MetadataRow<uint> {Column1 = 10},
                new MetadataRow<uint> {Column1 = 20},
                new MetadataRow<uint> {Column1 = 30},
                new MetadataRow<uint> {Column1 = 40},
                new MetadataRow<uint> {Column1 = 50},
            };

            const uint newValue = 60;
            sortedTable.Add(new MetadataRow<uint> {Column1 = newValue});
            
            Assert.Equal(6, sortedTable.Count);
            Assert.Equal(newValue, sortedTable[5].Column1);
        }

        [Fact]
        public void InsertAfterDuplicates()
        {
            var sortedTable = new DummySortedTable
            {
                new MetadataRow<uint> {Column1 = 10},
                new MetadataRow<uint> {Column1 = 30},
                new MetadataRow<uint> {Column1 = 30},
                new MetadataRow<uint> {Column1 = 30},
                new MetadataRow<uint> {Column1 = 30},
            };

            const uint newValue = 60;
            sortedTable.Add(new MetadataRow<uint> {Column1 = newValue});
            
            Assert.Equal(6, sortedTable.Count);
            Assert.Equal(newValue, sortedTable[5].Column1);
        }

        [Fact]
        public void InsertBeforeDuplicates()
        {
            var sortedTable = new DummySortedTable
            {
                new MetadataRow<uint> {Column1 = 30},
                new MetadataRow<uint> {Column1 = 30},
                new MetadataRow<uint> {Column1 = 30},
                new MetadataRow<uint> {Column1 = 30},
                new MetadataRow<uint> {Column1 = 60},
            };

            const uint newValue = 10;
            sortedTable.Add(new MetadataRow<uint> {Column1 = newValue});
            
            Assert.Equal(6, sortedTable.Count);
            Assert.Equal(newValue, sortedTable[0].Column1);
        }
    }
}