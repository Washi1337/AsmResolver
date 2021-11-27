using System;
using System.Text;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides methods for writing data to a binary stream.
    /// </summary>
    public interface IBinaryStreamWriter
    {
        /// <summary>
        /// Gets or sets the current position of the writer.
        /// </summary>
        ulong Offset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current length of the stream.
        /// </summary>
        uint Length
        {
            get;
        }

        /// <summary>
        /// Writes a buffer of data to the stream.
        /// </summary>
        /// <param name="buffer">The buffer to write to the stream.</param>
        /// <param name="startIndex">The index to start reading from the buffer.</param>
        /// <param name="count">The amount of bytes of the buffer to write.</param>
        void WriteBytes(byte[] buffer, int startIndex, int count);

        /// <summary>
        /// Writes a single byte to the stream.
        /// </summary>
        /// <param name="value">The byte to write.</param>
        void WriteByte(byte value);

        /// <summary>
        /// Writes an unsigned 16-bit integer to the stream.
        /// </summary>
        /// <param name="value">The unsigned 16-bit integer to write.</param>
        void WriteUInt16(ushort value);

        /// <summary>
        /// Writes an unsigned 32-bit integer to the stream.
        /// </summary>
        /// <param name="value">The unsigned 32-bit integer to write.</param>
        void WriteUInt32(uint value);

        /// <summary>
        /// Writes an unsigned 64-bit integer to the stream.
        /// </summary>
        /// <param name="value">The unsigned 64-bit integer to write.</param>
        void WriteUInt64(ulong value);

        /// <summary>
        /// Writes an signed byte to the stream.
        /// </summary>
        /// <param name="value">The signed byte to write.</param>
        void WriteSByte(sbyte value);

        /// <summary>
        /// Writes a signed 16-bit integer to the stream.
        /// </summary>
        /// <param name="value">The signed 16-bit integer to write.</param>
        void WriteInt16(short value);

        /// <summary>
        /// Writes a signed 32-bit integer to the stream.
        /// </summary>
        /// <param name="value">The signed 32-bit integer to write.</param>
        void WriteInt32(int value);

        /// <summary>
        /// Writes a signed 64-bit integer to the stream.
        /// </summary>
        /// <param name="value">The signed 64-bit integer to write.</param>
        void WriteInt64(long value);

        /// <summary>
        /// Writes a 32-bit floating point number to the stream.
        /// </summary>
        /// <param name="value">The 32-bit floating point number to write.</param>
        void WriteSingle(float value);

        /// <summary>
        /// Writes a 64-bit floating point number to the stream.
        /// </summary>
        /// <param name="value">The 64-bit floating point number to write.</param>
        void WriteDouble(double value);

        /// <summary>
        /// Writes a 128-bit decimal value to the stream.
        /// </summary>
        /// <param name="value">The 128-bit decimal value to write.</param>
        void WriteDecimal(decimal value);
    }

    /// <summary>
    /// Provides extension methods to various I/O interfaces in AsmResolver.
    /// </summary>
    public static partial class IOExtensions
    {
        /// <summary>
        /// Writes either a 32-bit or a 64-bit number to the output stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="is32Bit">Indicates the integer to be written is 32-bit or 64-bit.</param>
        /// <returns>The read number, zero extended if necessary.</returns>
        public static void WriteNativeInt(this IBinaryStreamWriter writer, ulong value, bool is32Bit)
        {
            if (is32Bit)
                writer.WriteUInt32((uint) value);
            else
                writer.WriteUInt64(value);
        }

        /// <summary>
        /// Writes a buffer of data to the stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="buffer">The data to write.</param>
        public static void WriteBytes(this IBinaryStreamWriter writer, byte[] buffer)
        {
            writer.WriteBytes(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes a specified amount of zero bytes to the stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="count">The amount of zeroes to write.</param>
        public static void WriteZeroes(this IBinaryStreamWriter writer, int count)
        {
            while (count >= sizeof(ulong))
            {
                writer.WriteUInt64(0);
                count -= sizeof(ulong);
            }

            while (count >= sizeof(uint))
            {
                writer.WriteUInt32(0);
                count -= sizeof(uint);
            }

            while (count >= sizeof(ushort))
            {
                writer.WriteUInt16(0);
                count -= sizeof(ushort);
            }

            while (count >= sizeof(byte))
            {
                writer.WriteByte(0);
                count -= sizeof(byte);
            }
        }

        /// <summary>
        /// Writes an ASCII string to the stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="value">The string to write.</param>
        public static void WriteAsciiString(this IBinaryStreamWriter writer, string value)
        {
            writer.WriteBytes(Encoding.ASCII.GetBytes(value));
        }

        /// <summary>
        /// Aligns the writer to a specified boundary.
        /// </summary>
        /// <param name="writer">The writer to align.</param>
        /// <param name="align">The boundary to use.</param>
        public static void Align(this IBinaryStreamWriter writer, uint align)
        {
            ulong currentPosition = writer.Offset;
            writer.WriteZeroes((int) (currentPosition.Align(align) - writer.Offset));
        }

        /// <summary>
        /// Writes a single index to the output stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="value">The index to write.</param>
        /// <param name="size"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void WriteIndex(this IBinaryStreamWriter writer, uint value, IndexSize size)
        {
            switch (size)
            {
                case IndexSize.Short:
                    writer.WriteUInt16((ushort) value);
                    break;
                case IndexSize.Long:
                    writer.WriteUInt32(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), size, null);
            }
        }

        /// <summary>
        /// Compresses and writes an unsigned integer to the stream.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteCompressedUInt32(this IBinaryStreamWriter writer, uint value)
        {
            if (value < 0x80)
            {
                writer.WriteByte((byte) value);
            }
            else if (value < 0x4000)
            {
                writer.WriteByte((byte)(0x80 | value >> 8));
                writer.WriteByte((byte)(value & 0xFF));
            }
            else
            {
                writer.WriteByte((byte)(0x80 | 0x40 | value >> 0x18));
                writer.WriteByte((byte)(value >> 0x10 & 0xFF));
                writer.WriteByte((byte)(value >> 0x08 & 0xFF));
                writer.WriteByte((byte)(value & 0xFF));
            }
        }

        /// <summary>
        /// Writes a single 7-bit encoded 32-bit integer to the output stream.
        /// </summary>
        /// <param name="writer">The output stream.</param>
        /// <param name="value">The value to write.</param>
        public static void Write7BitEncodedInt32(this IBinaryStreamWriter writer, int value)
        {
            uint x = unchecked((uint) value);
            do
            {
                byte b = (byte) (x & 0x7F);

                if (x > 0x7F)
                    b |= 0x80;

                writer.WriteByte(b);
                x >>= 7;
            } while (x != 0);
        }

        /// <summary>
        /// Writes an UTF8 string to the stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="value">The string to write.</param>
        public static void WriteSerString(this IBinaryStreamWriter writer, string? value)
        {
            if (value is null)
            {
                writer.WriteByte(0xFF);
                return;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            writer.WriteCompressedUInt32((uint)bytes.Length);
            writer.WriteBytes(bytes);
        }

        /// <summary>
        /// Writes a serialized string using the UTF-8 encoding that is prefixed by a 7-bit encoded length header.
        /// </summary>
        /// <param name="writer">The output stream.</param>
        /// <param name="value">The string to write.</param>
        public static void WriteBinaryFormatterString(this IBinaryStreamWriter writer, string value) =>
            WriteBinaryFormatterString(writer, value, Encoding.UTF8);

        /// <summary>
        /// Writes a serialized string that is prefixed by a 7-bit encoded length header.
        /// </summary>
        /// <param name="writer">The output stream.</param>
        /// <param name="value">The string to write.</param>
        /// <param name="encoding">The encoding to use.</param>
        public static void WriteBinaryFormatterString(this IBinaryStreamWriter writer, string value, Encoding encoding)
        {
            byte[] data = encoding.GetBytes(value);
            writer.Write7BitEncodedInt32(data.Length);
            writer.WriteBytes(data);
        }

        /// <summary>
        /// Writes an UTF8 string to the stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="value">The string to write.</param>
        public static void WriteSerString(this IBinaryStreamWriter writer, Utf8String? value)
        {
            if (value is null)
            {
                writer.WriteByte(0xFF);
                return;
            }

            byte[] bytes = value.GetBytesUnsafe();
            writer.WriteCompressedUInt32((uint)bytes.Length);
            writer.WriteBytes(bytes);
        }
    }
}
