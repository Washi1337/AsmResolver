using System;
using System.IO;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides a <see cref="IDataSource"/> wrapper around a raw <see cref="Stream"/>.
    /// </summary>
    public sealed class StreamDataSource : IDataSource
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        , ISpanDataSource
#endif
    {
        private readonly Stream _stream;

        /// <summary>
        /// Creates a new instance of the <see cref="StreamDataSource"/> class.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        public StreamDataSource(Stream stream)
            : this(stream, 0uL)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="StreamDataSource"/> class.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        /// <param name="baseAddress">The base address to use.</param>
        public StreamDataSource(Stream stream, ulong baseAddress)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead)
                throw new ArgumentException("The provided stream must be readable.", nameof(stream));
            if (!stream.CanSeek)
                throw new ArgumentException("The provided stream must be seekable.", nameof(stream));
            BaseAddress = baseAddress;
        }

        /// <inheritdoc />
        public ulong BaseAddress
        {
            get;
        }

        /// <inheritdoc />
        public byte this[ulong address]
        {
            get
            {
                long offset = (long)address - (long)BaseAddress;
                if (offset < 0 || offset >= _stream.Length)
                    throw new IndexOutOfRangeException();

                _stream.Seek(offset, SeekOrigin.Begin);
                return (byte)_stream.ReadByte();
            }
        }

        /// <inheritdoc />
        public ulong Length => (ulong)_stream.Length;

        /// <inheritdoc />
        public bool IsValidAddress(ulong address) => address - BaseAddress < Length;

        /// <inheritdoc />
        public int ReadBytes(ulong address, byte[] buffer, int index, int count)
        {
            long offset = (long)address - (long)BaseAddress;
            if (offset < 0 || offset + count > _stream.Length)
                throw new ArgumentOutOfRangeException(nameof(address), "The address is out of range of the data source.");
            _stream.Seek(offset, SeekOrigin.Begin);
            return _stream.Read(buffer, index, count);
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <inheritdoc />
        public int ReadBytes(ulong address, Span<byte> buffer)
        {
            long offset = (long)address - (long)BaseAddress;
            if (offset < 0 || offset + buffer.Length > _stream.Length)
                throw new ArgumentOutOfRangeException(nameof(address), "The address is out of range of the data source.");
            _stream.Seek(offset, SeekOrigin.Begin);
            return _stream.Read(buffer);
        }
#endif
    }
}
