using System;
using System.IO;
using System.Text;

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
using System.Buffers.Binary;
#endif

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides methods for writing data to an output stream.
    /// </summary>
    public sealed class BinaryStreamWriter
    {
        private const int ZeroBufferLength = 16;
        private static readonly byte[] ZeroBuffer = new byte[ZeroBufferLength];

        // Buffer to reduce individual IO ops.
        // Initialize this buffer in reverse order to prevent JIT emitting range checks for every byte.
        private readonly byte[] _buffer = new byte[8];

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

        /// <summary>
        /// Gets or sets the current position of the
        /// </summary>
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

        /// <summary>
        /// Gets or sets the current length of the stream.
        /// </summary>
        public uint Length => (uint) BaseStream.Length;

        /// <summary>
        /// Writes a buffer of data to the stream.
        /// </summary>
        /// <param name="buffer">The buffer to write to the stream.</param>
        /// <param name="startIndex">The index to start reading from the buffer.</param>
        /// <param name="count">The amount of bytes of the buffer to write.</param>
        public void WriteBytes(byte[] buffer, int startIndex, int count)
        {
            BaseStream.Write(buffer, startIndex, count);
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Writes a buffer of data to the stream.
        /// </summary>
        /// <param name="buffer">The buffer to write to the stream.</param>
        public void WriteBytes(ReadOnlySpan<byte> buffer)
        {
            BaseStream.Write(buffer);
        }
#endif

        /// <summary>
        /// Writes a single byte to the stream.
        /// </summary>
        /// <param name="value">The byte to write.</param>
        public void WriteByte(byte value)
        {
            BaseStream.WriteByte(value);
        }

        /// <summary>
        /// Writes an unsigned 16-bit integer to the stream.
        /// </summary>
        /// <param name="value">The unsigned 16-bit integer to write.</param>
        public void WriteUInt16(ushort value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            BinaryPrimitives.WriteUInt16LittleEndian(_buffer, value);
#else
            _buffer[1] = (byte) ((value >> 8) & 0xFF);
            _buffer[0] = (byte) (value & 0xFF);
#endif
            BaseStream.Write(_buffer, 0, 2);
        }

        /// <summary>
        /// Writes an unsigned 32-bit integer to the stream.
        /// </summary>
        /// <param name="value">The unsigned 32-bit integer to write.</param>
        public void WriteUInt32(uint value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            BinaryPrimitives.WriteUInt32LittleEndian(_buffer, value);
#else
            _buffer[3] = (byte) ((value >> 24) & 0xFF);
            _buffer[2] = (byte) ((value >> 16) & 0xFF);
            _buffer[1] = (byte) ((value >> 8) & 0xFF);
            _buffer[0] = (byte) (value & 0xFF);
#endif
            BaseStream.Write(_buffer, 0, 4);
        }

        /// <summary>
        /// Writes an unsigned 64-bit integer to the stream.
        /// </summary>
        /// <param name="value">The unsigned 64-bit integer to write.</param>
        public void WriteUInt64(ulong value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            BinaryPrimitives.WriteUInt64LittleEndian(_buffer, value);
#else
            _buffer[7] = (byte) ((value >> 56) & 0xFF);
            _buffer[6] = (byte) ((value >> 48) & 0xFF);
            _buffer[5] = (byte) ((value >> 40) & 0xFF);
            _buffer[4] = (byte) ((value >> 32) & 0xFF);
            _buffer[3] = (byte) ((value >> 24) & 0xFF);
            _buffer[2] = (byte) ((value >> 16) & 0xFF);
            _buffer[1] = (byte) ((value >> 8) & 0xFF);
            _buffer[0] = (byte) (value & 0xFF);
#endif
            BaseStream.Write(_buffer, 0, 8);
        }

        /// <summary>
        /// Writes an signed byte to the stream.
        /// </summary>
        /// <param name="value">The signed byte to write.</param>
        public void WriteSByte(sbyte value)
        {
            BaseStream.WriteByte(unchecked((byte) value));
        }

        /// <summary>
        /// Writes a signed 16-bit integer to the stream.
        /// </summary>
        /// <param name="value">The signed 16-bit integer to write.</param>
        public void WriteInt16(short value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            BinaryPrimitives.WriteInt16LittleEndian(_buffer, value);
#else
            _buffer[1] = (byte) ((value >> 8) & 0xFF);
            _buffer[0] = (byte) (value & 0xFF);
#endif
            BaseStream.Write(_buffer, 0, 2);
        }

        /// <summary>
        /// Writes a signed 32-bit integer to the stream.
        /// </summary>
        /// <param name="value">The signed 32-bit integer to write.</param>
        public void WriteInt32(int value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            BinaryPrimitives.WriteInt32LittleEndian(_buffer, value);
#else
            _buffer[3] = (byte) ((value >> 24) & 0xFF);
            _buffer[2] = (byte) ((value >> 16) & 0xFF);
            _buffer[1] = (byte) ((value >> 8) & 0xFF);
            _buffer[0] = (byte) (value & 0xFF);
#endif
            BaseStream.Write(_buffer, 0, 4);
        }

        /// <summary>
        /// Writes a signed 64-bit integer to the stream.
        /// </summary>
        /// <param name="value">The signed 64-bit integer to write.</param>
        public void WriteInt64(long value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            BinaryPrimitives.WriteInt64LittleEndian(_buffer, value);
#else
            _buffer[7] = (byte) ((value >> 56) & 0xFF);
            _buffer[6] = (byte) ((value >> 48) & 0xFF);
            _buffer[5] = (byte) ((value >> 40) & 0xFF);
            _buffer[4] = (byte) ((value >> 32) & 0xFF);
            _buffer[3] = (byte) ((value >> 24) & 0xFF);
            _buffer[2] = (byte) ((value >> 16) & 0xFF);
            _buffer[1] = (byte) ((value >> 8) & 0xFF);
            _buffer[0] = (byte) (value & 0xFF);
#endif
            BaseStream.Write(_buffer, 0, 8);
        }

        /// <summary>
        /// Writes a 32-bit floating point number to the stream.
        /// </summary>
        /// <param name="value">The 32-bit floating point number to write.</param>
        public unsafe void WriteSingle(float value)
        {
            WriteUInt32(*(uint*) &value);
        }

        /// <summary>
        /// Writes a 64-bit floating point number to the stream.
        /// </summary>
        /// <param name="value">The 64-bit floating point number to write.</param>
        public unsafe void WriteDouble(double value)
        {
            WriteUInt64(*(ulong*) &value);
        }

        /// <summary>
        /// Writes a 128-bit decimal value to the stream.
        /// </summary>
        /// <param name="value">The 128-bit decimal value to write.</param>
        public void WriteDecimal(decimal value)
        {
            int[] bits = decimal.GetBits(value);
            WriteInt32(bits[0]);
            WriteInt32(bits[1]);
            WriteInt32(bits[2]);
            WriteInt32(bits[3]);
        }

        /// <summary>
        /// Writes either a 32-bit or a 64-bit number to the output stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="is32Bit">Indicates the integer to be written is 32-bit or 64-bit.</param>
        /// <returns>The read number, zero extended if necessary.</returns>
        public void WriteNativeInt(ulong value, bool is32Bit)
        {
            if (is32Bit)
                WriteUInt32((uint) value);
            else
                WriteUInt64(value);
        }

        /// <summary>
        /// Writes a buffer of data to the stream.
        /// </summary>
        /// <param name="buffer">The data to write.</param>
        public void WriteBytes(byte[] buffer)
        {
            WriteBytes(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes a specified number of zero bytes to the stream.
        /// </summary>
        /// <param name="count">The number of zeroes to write.</param>
        public void WriteZeroes(int count)
        {
            while (count >= ZeroBufferLength)
            {
                BaseStream.Write(ZeroBuffer, 0, ZeroBufferLength);
                count -= ZeroBufferLength;
            }

            if (count > 0)
                BaseStream.Write(ZeroBuffer, 0, Math.Min(count, ZeroBufferLength));
        }

        /// <summary>
        /// Writes an ASCII string to the stream.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public void WriteAsciiString(string value)
        {
            WriteBytes(Encoding.ASCII.GetBytes(value));
        }

        /// <summary>
        /// Aligns the writer to a specified boundary.
        /// </summary>
        /// <param name="align">The boundary to use.</param>
        public void Align(uint align) => AlignRelative(align, 0);

        /// <summary>
        /// Aligns the writer to a specified boundary, relative to the provided start offset..
        /// </summary>
        /// <param name="align">The boundary to use.</param>
        /// <param name="startOffset">The starting offset to consider the alignment boundaries from.</param>
        public void AlignRelative(uint align, ulong startOffset)
        {
            ulong currentPosition = Offset - startOffset;
            WriteZeroes((int) (currentPosition.Align(align) - currentPosition));
        }

        /// <summary>
        /// Writes a single index to the output stream.
        /// </summary>
        /// <param name="value">The index to write.</param>
        /// <param name="size"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void WriteIndex(uint value, IndexSize size)
        {
            switch (size)
            {
                case IndexSize.Short:
                    WriteUInt16((ushort) value);
                    break;

                case IndexSize.Long:
                    WriteUInt32(value);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(size), size, null);
            }
        }

        /// <summary>
        /// Compresses and writes an unsigned integer to the stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteCompressedUInt32(uint value)
        {
            switch (value)
            {
                case < 0x80:
                    WriteByte((byte) value);
                    break;

                case < 0x4000:
                    _buffer[1] = (byte) value;
                    _buffer[0] = (byte) (0x80 | value >> 8);
                    BaseStream.Write(_buffer, 0, 2);
                    break;

                default:
                    _buffer[3] = (byte) value;
                    _buffer[2] = (byte) (value >> 0x08);
                    _buffer[1] = (byte) (value >> 0x10);
                    _buffer[0] = (byte) (0x80 | 0x40 | value >> 0x18);
                    BaseStream.Write(_buffer, 0, 4);
                    break;
            }
        }

        /// <summary>
        /// Compresses and writes a signed integer to the stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteCompressedInt32(int value)
        {
            uint sign = (uint) value >> 31;
            uint rotated;

            switch (value)
            {
                case >= -0x40 and < 0x40:
                    rotated = ((uint) (value & 0x3F) << 1) | sign;
                    WriteByte((byte) rotated);
                    break;

                case >= -0x2000 and < 0x2000:
                    rotated = ((uint) (value & 0x1FFF) << 1) | sign;
                    _buffer[1] = (byte) rotated;
                    _buffer[0] = (byte) (0x80 | rotated >> 8);
                    BaseStream.Write(_buffer, 0, 2);
                    break;

                default:
                    rotated = ((uint) (value & 0x0FFF_FFFF) << 1) | sign;
                    _buffer[3] = (byte) rotated;
                    _buffer[2] = (byte) (rotated >> 0x08);
                    _buffer[1] = (byte) (rotated >> 0x10);
                    _buffer[0] = (byte)(0x80 | 0x40 | rotated >> 0x18);
                    BaseStream.Write(_buffer, 0, 4);
                    break;
            }
        }

        /// <summary>
        /// Writes a single 7-bit encoded 32-bit integer to the output stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write7BitEncodedInt32(int value)
        {
            uint x = unchecked((uint) value);
            do
            {
                byte b = (byte) (x & 0x7F);

                if (x > 0x7F)
                    b |= 0x80;

                WriteByte(b);
                x >>= 7;
            } while (x != 0);
        }

        /// <summary>
        /// Writes an UTF8 string to the stream.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public void WriteSerString(string? value)
        {
            if (value is null)
            {
                WriteByte(0xFF);
                return;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteCompressedUInt32((uint)bytes.Length);
            WriteBytes(bytes);
        }

        /// <summary>
        /// Writes a serialized string using the UTF-8 encoding that is prefixed by a 7-bit encoded length header.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public void WriteBinaryFormatterString(string value) => WriteBinaryFormatterString(value, Encoding.UTF8);

        /// <summary>
        /// Writes a serialized string that is prefixed by a 7-bit encoded length header.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <param name="encoding">The encoding to use.</param>
        public void WriteBinaryFormatterString(string value, Encoding encoding)
        {
            byte[] data = encoding.GetBytes(value);
            Write7BitEncodedInt32(data.Length);
            WriteBytes(data);
        }

        /// <summary>
        /// Writes an UTF8 string to the stream.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public void WriteSerString(Utf8String? value)
        {
            if (value is null)
            {
                WriteByte(0xFF);
                return;
            }

            byte[] bytes = value.GetBytesUnsafe();
            WriteCompressedUInt32((uint)bytes.Length);
            WriteBytes(bytes);
        }
    }
}
