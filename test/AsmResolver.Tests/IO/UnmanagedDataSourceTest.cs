using System;
using System.Runtime.InteropServices;
using AsmResolver.IO;
using Xunit;

namespace AsmResolver.Tests.IO
{
    public unsafe class UnmanagedDataSourceTest : IDisposable
    {
        private readonly IntPtr _pointer;
        private readonly UnmanagedDataSource _source;

        public UnmanagedDataSourceTest()
        {
            const int testDataLength = 100;

            _pointer = Marshal.AllocHGlobal(testDataLength);
            for (int i = 0; i < testDataLength; i++)
                ((byte*) _pointer)[i] = (byte) (i & 0xFF);

            _source = new UnmanagedDataSource(_pointer.ToPointer(), testDataLength);
        }

        [Fact]
        public void ValidAddresses()
        {
            Assert.False(_source.IsValidAddress(0));
            Assert.True(_source.IsValidAddress(_source.BaseAddress));
            Assert.True(_source.IsValidAddress(_source.BaseAddress + _source.Length - 1));
            Assert.False(_source.IsValidAddress(_source.BaseAddress + _source.Length));
        }

        [Fact]
        public void ReadSingleValidByte()
        {
            Assert.Equal(((byte*) _pointer)[0], _source[_source.BaseAddress]);
            Assert.Equal(((byte*) _pointer)[_source.Length - 1], _source[_source.BaseAddress + _source.Length - 1]);
        }

        [Fact]
        public void ReadSingleInvalidByte()
        {
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => _source[0]);
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => _source[_source.BaseAddress + _source.Length]);
        }

        [Fact]
        public void ReadMultipleBytesFullyValid()
        {
            byte[] expected = new byte[50];
            Marshal.Copy(_pointer, expected, 0, expected.Length);

            byte[] actual = new byte[50];
            Assert.Equal(actual.Length, _source.ReadBytes(_source.BaseAddress, actual, 0, actual.Length));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadMultipleBytesHalfValid()
        {
            byte[] expected = new byte[50];
            Marshal.Copy(
                (IntPtr) ((ulong) _pointer + _source.Length - (ulong) (expected.Length / 2)), expected,
                0,
                expected.Length / 2);

            byte[] actual = new byte[50];
            Assert.Equal(actual.Length / 2,
                _source.ReadBytes(
                    _source.BaseAddress + _source.Length - (ulong) (expected.Length / 2),
                    actual,
                    0,
                    actual.Length));

            Assert.Equal(expected, actual);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Marshal.FreeHGlobal(_pointer);
        }
    }
}
