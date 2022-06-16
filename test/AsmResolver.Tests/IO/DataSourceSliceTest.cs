using System;
using System.Linq;
using AsmResolver.IO;
using Xunit;

namespace AsmResolver.Tests.IO
{
    public class DataSourceSliceTest
    {
        private readonly IDataSource _source = new ByteArrayDataSource(new byte[]
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
        });

        [Fact]
        public void EmptySlice()
        {
            var slice = new DataSourceSlice(_source, 0, 0);
            Assert.Equal(0ul, slice.Length);
        }

        [Fact]
        public void SliceStart()
        {
            var slice = new DataSourceSlice(_source, 0, 5);
            Assert.Equal(5ul, slice.Length);
            Assert.All(Enumerable.Range(0, 5), i => Assert.Equal(slice[(ulong) i], _source[(ulong) i]));
            Assert.Throws<IndexOutOfRangeException>(() => slice[5]);
        }

        [Fact]
        public void SliceMiddle()
        {
            var slice = new DataSourceSlice(_source, 3, 5);
            Assert.Equal(5ul, slice.Length);
            Assert.All(Enumerable.Range(3, 5), i => Assert.Equal(slice[(ulong) i], _source[(ulong) i]));
            Assert.Throws<IndexOutOfRangeException>(() => slice[3 - 1]);
            Assert.Throws<IndexOutOfRangeException>(() => slice[3 + 5]);
        }

        [Fact]
        public void SliceEnd()
        {
            var slice = new DataSourceSlice(_source, 5, 5);
            Assert.Equal(5ul, slice.Length);
            Assert.All(Enumerable.Range(5, 5), i => Assert.Equal(slice[(ulong) i], _source[(ulong) i]));
            Assert.Throws<IndexOutOfRangeException>(() => slice[5 - 1]);
        }

        [Fact]
        public void ReadSlicedShouldReadUpToSliceAmountOfBytes()
        {
            var slice = new DataSourceSlice(_source, 3, 5);

            byte[] data1 = new byte[7];
            int originalCount = _source.ReadBytes(3, data1, 0, data1.Length);
            Assert.Equal(7, originalCount);

            byte[] data2 = new byte[3];
            int newCount = slice.ReadBytes(3, data2, 0, data2.Length);
            Assert.Equal(3, newCount);
            Assert.Equal(data1.Take(3), data2.Take(3));

            byte[] data3 = new byte[7];
            int newCount2 = slice.ReadBytes(3, data3, 0, data3.Length);
            Assert.Equal(5, newCount2);
            Assert.Equal(data1.Take(5), data3.Take(5));
        }
    }
}
