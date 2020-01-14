using System.Collections.Generic;
using AsmResolver.DotNet.Builder.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder.Tables
{
    public class UnsortedMetadataTableBufferTest
    {
        private readonly UnsortedMetadataTableBuffer<DummyRow> _buffer;

        public UnsortedMetadataTableBufferTest()
        {
            _buffer = new UnsortedMetadataTableBuffer<DummyRow>(
                new TableLayout(new ColumnLayout("Id", ColumnType.UInt32)));
        }
        
        [Fact]
        public void EmptyTable()
        {
            var info = _buffer.CreateTable();
            Assert.Empty(info.ConstructedTable);
        }

        [Fact]
        public void AddSingleItem()
        {
            var row = new DummyRow(1234);
            var handle = _buffer.Add(row);
            var info = _buffer.CreateTable();

            Assert.Equal(new[]
            {
                row
            }, info.ConstructedTable);
            Assert.Equal(row, info.ConstructedTable.GetByRid(info.GetRid(handle)));
        }

        [Fact]
        public void AddMultipleItems()
        {
            var rows = new[]
            {
                new DummyRow(0xA),
                new DummyRow(0xB),
                new DummyRow(0xC),
                new DummyRow(0xD),
            };
            
            var handles = new List<MetadataRowHandle>();
            foreach (var row in rows)
                handles.Add(_buffer.Add(row));
            
            var info = _buffer.CreateTable();

            Assert.Equal(rows, info.ConstructedTable);
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                var handle = handles[i];
                Assert.Equal(row, info.ConstructedTable.GetByRid(info.GetRid(handle)));
            }
        }
        
    }
}