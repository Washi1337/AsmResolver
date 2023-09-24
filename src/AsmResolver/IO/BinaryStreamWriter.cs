using System;
using System.IO;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides a default implementation of a binary writer that writes the data to an output stream.
    /// </summary>
    public class BinaryStreamWriter : IBinaryStreamWriter
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        , ISpanBinaryStreamWriter
#endif
    {
        /// <summary>
        /// Creates a new binary stream writer using the provided output stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        public BinaryStreamWriter(Stream stream)
        {
            BaseStream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        /// <summary>
        /// Gets the stream this writer writes to.
        /// </summary>
        public Stream BaseStream
        {
            get;
        }

        /// <inheritdoc />
        public ulong Offset
        {
            get => (uint) BaseStream.Position;
            set
            {
                // Check if position actually changed before actually setting. If we don't do this, this can cause
                // performance issues on some systems. See https://github.com/Washi1337/AsmResolver/issues/232
                if (BaseStream.Position != (long) value)
                    BaseStream.Position = (long) value;
            }
        }

        /// <inheritdoc />
        public uint Length => (uint) BaseStream.Length;

        /// <inheritdoc />
        public void WriteBytes(byte[] buffer, int startIndex, int count)
        {
            BaseStream.Write(buffer, startIndex, count);
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <inheritdoc />
        public void WriteBytes(ReadOnlySpan<byte> buffer)
        {
            BaseStream.Write(buffer);
        }
#endif

        /// <inheritdoc />
        public void WriteByte(byte value)
        {
            BaseStream.WriteByte(value);
        }

        /// <inheritdoc />
        public void WriteUInt16(ushort value)
        {
            BaseStream.WriteByte((byte) (value & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 8) & 0xFF));
        }

        /// <inheritdoc />
        public void WriteUInt32(uint value)
        {
            BaseStream.WriteByte((byte) (value & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 8) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 16) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 24) & 0xFF));
        }

        /// <inheritdoc />
        public void WriteUInt64(ulong value)
        {
            BaseStream.WriteByte((byte) (value & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 8) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 16) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 24) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 32) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 40) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 48) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 56) & 0xFF));
        }

        /// <inheritdoc />
        public void WriteSByte(sbyte value)
        {
            BaseStream.WriteByte(unchecked((byte) value));
        }

        /// <inheritdoc />
        public void WriteInt16(short value)
        {
            BaseStream.WriteByte((byte) (value & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 8) & 0xFF));
        }

        /// <inheritdoc />
        public void WriteInt32(int value)
        {
            BaseStream.WriteByte((byte) (value & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 8) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 16) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 24) & 0xFF));
        }

        /// <inheritdoc />
        public void WriteInt64(long value)
        {
            BaseStream.WriteByte((byte) (value & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 8) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 16) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 24) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 32) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 40) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 48) & 0xFF));
            BaseStream.WriteByte((byte) ((value >> 56) & 0xFF));
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

        /// <inheritdoc />
        public void WriteDecimal(decimal value)
        {
            int[] bits = decimal.GetBits(value);
            WriteInt32(bits[0]);
            WriteInt32(bits[1]);
            WriteInt32(bits[2]);
            WriteInt32(bits[3]);
        }
    }
}
