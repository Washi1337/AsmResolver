using System;
using System.IO;
using System.Linq;
using AsmResolver.IO;
using Xunit;

namespace AsmResolver.Tests.IO
{
    public class MemoryStreamReaderPoolTest
    {
        private readonly MemoryStreamWriterPool _pool = new();

        [Fact]
        public void RentShouldStartWithEmptyStream()
        {
            using (var rent = _pool.Rent())
            {
                Assert.Equal(0u, rent.Writer.Length);
                rent.Writer.WriteInt64(0x0123456789abcdef);
                Assert.Equal(8u, rent.Writer.Length);
            }

            using (var rent = _pool.Rent())
            {
                Assert.Equal(0u, rent.Writer.Length);
            }
        }

        [Fact]
        public void RentBeforeDisposeShouldUseNewBackendStream()
        {
            using var rent1 = _pool.Rent();
            using var rent2 = _pool.Rent();
            Assert.NotSame(rent1.Writer.BaseStream, rent2.Writer.BaseStream);
        }

        [Fact]
        public void RentAfterDisposeShouldReuseBackendStream()
        {
            Stream stream;
            using (var rent = _pool.Rent())
            {
                stream = rent.Writer.BaseStream;
            }

            using (var rent = _pool.Rent())
            {
                Assert.Same(stream, rent.Writer.BaseStream);
            }
        }

        [Fact]
        public void RentAfterDisposeShouldReuseBackendStream2()
        {
            var rent1 = _pool.Rent();
            var rent2 = _pool.Rent();
            var stream2 = rent2.Writer.BaseStream;
            var rent3 = _pool.Rent();

            rent2.Dispose();
            var rent4 = _pool.Rent();

            Assert.Same(stream2, rent4.Writer.BaseStream);
        }

        [Fact]
        public void GetFinalData()
        {
            byte[] value = Enumerable.Range(0, 255)
                .Select(x => (byte) x)
                .ToArray();

            using var rent = _pool.Rent();
            rent.Writer.WriteBytes(value);
            rent.Writer.WriteBytes(value);
            Assert.Equal(value.Concat(value), rent.GetData());
        }

        [Fact]
        public void UseAfterDisposeShouldThrow()
        {
            Assert.Throws<ObjectDisposedException>(() =>
            {
                var rent = _pool.Rent();
                rent.Dispose();
                rent.Writer.WriteInt64(0);
            });

            Assert.Throws<ObjectDisposedException>(() =>
            {
                var rent = _pool.Rent();
                rent.Dispose();
                return rent.GetData();
            });
        }

        [Fact]
        public void DisposeTwiceShouldNotReturnTwice()
        {
            var rent1 = _pool.Rent();
            var stream = rent1.Writer.BaseStream;

            rent1.Dispose();
            rent1.Dispose();

            var rent2 = _pool.Rent();
            var rent3 = _pool.Rent();

            Assert.Same(stream, rent2.Writer.BaseStream);
            Assert.NotSame(stream, rent3.Writer.BaseStream);
        }

        [Fact]
        public void GetDataShouldNotResultInSameInstance()
        {
            byte[] data1;

            using (var rent1 = _pool.Rent())
            {
                rent1.Writer.WriteInt64(0x0123456789abcdef);
                data1 = rent1.GetData();
                byte[] data2 = rent1.GetData();
                Assert.Equal(data1, data2);
                Assert.NotSame(data1, data2);
            }

            using (var rent2 = _pool.Rent())
            {
                rent2.Writer.WriteInt64(0x0123456789abcdef);
                byte[] data3 = rent2.GetData();
                Assert.Equal(data1, data3);
                Assert.NotSame(data1, data3);
            }
        }
    }
}
