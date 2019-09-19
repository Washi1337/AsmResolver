using System;
using System.IO;

namespace AsmResolver
{
    /// <summary>
    /// Provides a default implementation of a binary writer that writes the data to an output stream.
    /// </summary>
    public class BinaryStreamWriter : IBinaryStreamWriter
    {
        private readonly Stream _stream;

        /// <summary>
        /// Creates a new binary stream writer using the provided output stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        public BinaryStreamWriter(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        /// <inheritdoc />
        public uint FileOffset
        {
            get => (uint) _stream.Position;
            set => _stream.Position = value;
        }

        /// <inheritdoc />
        public uint Length => (uint) _stream.Length;

        /// <inheritdoc />
        public void WriteBytes(byte[] buffer, int startIndex, int count)
        {
            _stream.Write(buffer, startIndex, count);
        }

        /// <inheritdoc />
        public void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        /// <inheritdoc />
        public void WriteUInt16(ushort value)
        {
            _stream.WriteByte((byte) (value & 0xFF));
            _stream.WriteByte((byte) ((value >> 8) & 0xFF));
        }

        /// <inheritdoc />
        public void WriteUInt32(uint value)
        {
            _stream.WriteByte((byte) (value & 0xFF));
            _stream.WriteByte((byte) ((value >> 8) & 0xFF));
            _stream.WriteByte((byte) ((value >> 16) & 0xFF));
            _stream.WriteByte((byte) ((value >> 24) & 0xFF));
        }

        /// <inheritdoc />
        public void WriteUInt64(ulong value)
        {
            _stream.WriteByte((byte) (value & 0xFF));
            _stream.WriteByte((byte) ((value >> 8) & 0xFF));
            _stream.WriteByte((byte) ((value >> 16) & 0xFF));
            _stream.WriteByte((byte) ((value >> 24) & 0xFF));
            _stream.WriteByte((byte) ((value >> 32) & 0xFF));
            _stream.WriteByte((byte) ((value >> 40) & 0xFF));
            _stream.WriteByte((byte) ((value >> 48) & 0xFF));
            _stream.WriteByte((byte) ((value >> 56) & 0xFF));
        }

        /// <inheritdoc />
        public void WriteSByte(sbyte value)
        {
            _stream.WriteByte(unchecked((byte) value));
        }

        /// <inheritdoc />
        public void WriteInt16(short value)
        {
            _stream.WriteByte((byte) (value & 0xFF));
            _stream.WriteByte((byte) ((value >> 8) & 0xFF));
        }

        /// <inheritdoc />
        public void WriteInt32(int value)
        {
            _stream.WriteByte((byte) (value & 0xFF));
            _stream.WriteByte((byte) ((value >> 8) & 0xFF));
            _stream.WriteByte((byte) ((value >> 16) & 0xFF));
            _stream.WriteByte((byte) ((value >> 24) & 0xFF));
        }

        /// <inheritdoc />
        public void WriteInt64(long value)
        {
            _stream.WriteByte((byte) (value & 0xFF));
            _stream.WriteByte((byte) ((value >> 8) & 0xFF));
            _stream.WriteByte((byte) ((value >> 16) & 0xFF));
            _stream.WriteByte((byte) ((value >> 24) & 0xFF));
            _stream.WriteByte((byte) ((value >> 32) & 0xFF));
            _stream.WriteByte((byte) ((value >> 40) & 0xFF));
            _stream.WriteByte((byte) ((value >> 48) & 0xFF));
            _stream.WriteByte((byte) ((value >> 56) & 0xFF));
        }

        /// <inheritdoc />
        public unsafe void WriteSingle(float value)
        {
            WriteUInt32(*(uint*) &value);
        }

        /// <inheritdoc />
        public unsafe void WriteDouble(double value)
        {
            WriteUInt64(*(ulong*) &value);
        }
        
    }
}