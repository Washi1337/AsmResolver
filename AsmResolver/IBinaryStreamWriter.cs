using System.Text;
using AsmResolver.Net;

namespace AsmResolver
{
    /// <summary>
    /// Provides methods for writing data to a binary stream.
    /// </summary>
    public interface IBinaryStreamWriter
    {
        /// <summary>
        /// Gets or sets the current position of the writer.
        /// </summary>
        long Position
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current length of the stream.
        /// </summary>
        long Length
        {
            get;
        }
        
        /// <summary>
        /// Writes a buffer of data to the stream.
        /// </summary>
        /// <param name="buffer">The buffer to write to the stream.</param>
        /// <param name="count">The amount of bytes of the buffer to write.</param>
        void WriteBytes(byte[] buffer, int count);

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

    }

    /// <summary>
    /// Provides extensions for the <see cref="IBinaryStreamWriter"/> interface.
    /// </summary>
    public static class OutputStreamExtensions
    {
        /// <summary>
        /// Writes a buffer of data to the stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="buffer">The data to write.</param>
        public static void WriteBytes(this IBinaryStreamWriter writer, byte[] buffer)
        {
            writer.WriteBytes(buffer, buffer.Length);
        }

        /// <summary>
        /// Writes a specified amount of zero bytes to the stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="count">The amount of zeroes to write.</param>
        public static void WriteZeroes(this IBinaryStreamWriter writer, int count)
        {
            writer.WriteBytes(new byte[count]);
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
        /// Writes an UTF8 string to the stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="value">The string to write.</param>
        public static void WriteSerString(this IBinaryStreamWriter writer, string value)
        {
            if (value == null)
            {
                writer.WriteByte(0xFF);
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(value);
            writer.WriteCompressedUInt32((uint)bytes.Length);
            writer.WriteBytes(bytes);
        }

        /// <summary>
        /// Determines the size (in bytes) of an integer when it is compressed.
        /// </summary>
        /// <param name="value">The value to get the size from.</param>
        /// <returns>The amount of bytes.</returns>
        public static uint GetCompressedSize(this uint value)
        {
            if (value < 0x80)
                return 1;
            if (value < 0x4000)
                return 2;
            return 4;
        }

        /// <summary>
        /// Determines the size (in bytes) of an integer when it is compressed.
        /// </summary>
        /// <param name="value">The value to get the size from.</param>
        /// <returns>The amount of bytes.</returns>
        public static uint GetCompressedSize(this int value)
        {
            return ((uint)value).GetCompressedSize();
        }

        /// <summary>
        /// Determines the size (in bytes) of a string.
        /// </summary>
        /// <param name="value">The string to get the size from.</param>
        /// <returns>The amount of bytes.</returns>
        public static uint GetSerStringSize(this string value)
        {
            var byteCount = (uint)Encoding.UTF8.GetByteCount(value);
            return byteCount.GetCompressedSize() + byteCount;
        }

        /// <summary>
        /// Compresses and writes an unsigned integer to the stream.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteCompressedUInt32(this IBinaryStreamWriter writer, uint value)
        {
            if (value < 0x80)
                writer.WriteByte((byte)value);
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
        /// Writes an index with the specified size to the stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="indexSize">The size of the index.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteIndex(this IBinaryStreamWriter writer, IndexSize indexSize, uint value)
        {
            if (indexSize == IndexSize.Short)
                writer.WriteUInt16((ushort)value);
            else
                writer.WriteUInt32(value);
        }

        /// <summary>
        /// Aligns the writer to a specified boundary.
        /// </summary>
        /// <param name="writer">The writer to align.</param>
        /// <param name="align">The boundary to use.</param>
        public static void Align(this IBinaryStreamWriter writer, int align)
        {
            align--;
            writer.WriteZeroes((((int)writer.Position + align) & ~align) - (int)writer.Position);
        }
    }
}
