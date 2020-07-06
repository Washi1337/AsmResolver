using System;
using System.IO;
using System.Text;

namespace AsmResolver
{
    /// <summary>
    /// Provides methods to read data from a binary stream.
    /// </summary>
    public interface IBinaryStreamReader
    {
        /// <summary>
        /// Gets the starting position of the reader.
        /// </summary>
        uint StartPosition
        {
            get;
        }

        /// <summary>
        /// Gets the starting virtual address of the reader.
        /// </summary>
        uint StartRva
        {
            get;
        }

        /// <summary>
        /// Gets the current position of the reader.
        /// </summary>
        uint FileOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the current relative virtual address of the reader.
        /// </summary>
        uint Rva
        {
            get;
        }

        /// <summary>
        /// Gets the maximum length of data the reader can read from the stream.
        /// </summary>
        uint Length
        {
            get;
        }

        /// <summary>
        /// Forks the reader by creating a new instance of a binary stream reader, using the same data source, but a different address and size.
        /// </summary>
        /// <param name="address">The address of the forked reader to start at.</param>
        /// <param name="size">The size of the data the forked reader can read.</param>
        /// <returns>A forked binary stream reader with the same data source, but a different address and size.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when <paramref name="address"/> does not fall within the range of the current reader.</exception>
        /// <exception cref="EndOfStreamException">Occurs when <paramref name="size"/> results in the new reader to reach out of the stream.</exception>
        IBinaryStreamReader Fork(uint address, uint size);

        void ChangeSize(uint newSize);

        /// <summary>
        /// Reads data from the stream until a specific byte is encountered.
        /// </summary>
        /// <param name="value">The byte to use as a delimeter.</param>
        /// <returns>The data that was read from the stream, including the delimeter.</returns>
        byte[] ReadBytesUntil(byte value);

        /// <summary>
        /// Reads a specific amount of bytes from the stream.
        /// </summary>
        /// <param name="buffer">The buffer to put the read data in.</param>
        /// <param name="startIndex">The index in the buffer to start writing in.</param>
        /// <param name="count">The amount of bytes to read.</param>
        /// <returns>The number of bytes that were read.</returns>
        int ReadBytes(byte[] buffer, int startIndex, int count);

        /// <summary>
        /// Reads a single byte from the stream.
        /// </summary>
        /// <returns>The byte that was read from the stream.</returns>
        byte ReadByte();

        /// <summary>
        /// Reads an unsigned 16-bit integer from the stream.
        /// </summary>
        /// <returns>The unsigned 16-bit integer that was read from the stream.</returns>
        ushort ReadUInt16();

        /// <summary>
        /// Reads an unsigned 32-bit integer from the stream.
        /// </summary>
        /// <returns>The unsigned 32-bit integer that was read from the stream.</returns>
        uint ReadUInt32();

        /// <summary>
        /// Reads an unsigned 64-bit integer from the stream.
        /// </summary>
        /// <returns>The unsigned 64-bit integer that was read from the stream.</returns>
        ulong ReadUInt64();

        /// <summary>
        /// Reads a signed 8-bit integer from the stream.
        /// </summary>
        /// <returns>The signed 8-bit integer that was read from the stream.</returns>
        sbyte ReadSByte();

        /// <summary>
        /// Reads a signed 16-bit integer from the stream.
        /// </summary>
        /// <returns>The signed 16-bit integer that was read from the stream.</returns>
        short ReadInt16();

        /// <summary>
        /// Reads a signed 32-bit integer from the stream.
        /// </summary>
        /// <returns>The signed 32-bit integer that was read from the stream.</returns>
        int ReadInt32();

        /// <summary>
        /// Reads a signed 64-bit integer from the stream.
        /// </summary>
        /// <returns>The signed 64-bit integer that was read from the stream.</returns>
        long ReadInt64();

        /// <summary>
        /// Reads a 32-bit floating point number from the stream.
        /// </summary>
        /// <returns>The 32-bit floating point number that was read from the stream.</returns>
        float ReadSingle();

        /// <summary>
        /// Reads a 64-bit floating point number from the stream.
        /// </summary>
        /// <returns>The 64-bit floating point number that was read from the stream.</returns>
        double ReadDouble();
        
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Determines whether the reader can read up to a specific amount of bytes.
        /// </summary>
        /// <param name="reader">The reader to check.</param>
        /// <param name="size">The size of the data to check.</param>
        /// <returns></returns>
        public static bool CanRead(this IBinaryStreamReader reader, int size)
        {
            return (reader.FileOffset - reader.StartPosition) + size <= reader.Length;
        }

        /// <summary>
        /// Reads all remaining data from the input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The remaining bytes of the input stream.</returns>
        public static byte[] ReadToEnd(this IBinaryStreamReader reader)
        {
            int remainingByteCount = (int) (reader.Length - (reader.FileOffset - reader.StartPosition));
            var buffer = new byte[remainingByteCount];
            int read = reader.ReadBytes(buffer, 0, remainingByteCount);
            if (read != remainingByteCount)
                Array.Resize(ref buffer, read);
            return buffer;
        }

        /// <summary>
        /// Forks the reader at th current position by creating a new instance of a binary stream reader, using the same data source.
        /// </summary>
        /// <param name="reader">The reader to fork.</param>
        /// <returns>A forked binary stream reader with the same data source.</returns>
        public static IBinaryStreamReader Fork(this IBinaryStreamReader reader)
        {
            uint address = reader.FileOffset;
            return reader.Fork(address, reader.Length - (address - reader.StartPosition));
        }

        /// <summary>
        /// Forks the reader by creating a new instance of a binary stream reader, using the same data source, but a different address.
        /// </summary>
        /// <param name="reader">The reader to fork.</param>
        /// <param name="address">The address of the forked reader to start at.</param>
        /// <returns>A forked binary stream reader with the same data source, but a different address.</returns>
        public static IBinaryStreamReader Fork(this IBinaryStreamReader reader, uint address)
        {
            return reader.Fork(address, reader.Length - (address - reader.StartPosition));
        }

        /// <summary>
        /// Reads a zero-terminated ASCII string from the stream.
        /// </summary>
        /// <param name="reader">The reader to use for reading the data.</param>
        /// <returns>The string that was read from the stream.</returns>
        public static string ReadAsciiString(this IBinaryStreamReader reader)
        {
            var data = reader.ReadBytesUntil(0);
            int length = data.Length;
            
            // Exclude trailing 0 byte.
            if (data[data.Length - 1] == 0)
                length--;
            
            return Encoding.ASCII.GetString(data, 0, length);
        }

        /// <summary>
        /// Reads a zero-terminated Unicode string from the stream.
        /// </summary>
        /// <param name="reader">The reader to use for reading the data.</param>
        /// <returns>The string that was read from the stream.</returns>
        public static string ReadUnicodeString(this IBinaryStreamReader reader)
        {
            var builder = new StringBuilder();

            while (true)
            {
                char nextChar = (char) reader.ReadUInt16();
                if (nextChar is '\0')
                    break;
                builder.Append(nextChar);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Reads an aligned ASCII string from the stream.
        /// </summary>
        /// <param name="reader">The reader to use for reading the data.</param>
        /// <param name="align">The alignment to use.</param>
        /// <returns>The string that was read from the stream.</returns>
        public static string ReadAlignedAsciiString(this IBinaryStreamReader reader, int align)
        {
            var position = reader.FileOffset;
            var value = reader.ReadAsciiString();
            do
            {
                reader.FileOffset++;
            } while ((reader.FileOffset - position) % align != 0);
            return value;
        }
        
        /// <summary>
        /// Aligns the reader to a specified boundary.
        /// </summary>
        /// <param name="reader">The reader to align.</param>
        /// <param name="alignment">The boundary to use.</param>
        public static void Align(this IBinaryStreamReader reader, uint alignment)
        {
            reader.FileOffset = reader.FileOffset.Align(alignment);
        }

        /// <summary>
        /// Either reads a 32 bit number or a 64 bit number.
        /// </summary>
        /// <param name="reader">The reader to use for reading the data.</param>
        /// <param name="is32Bit">Determines whether to read a 32 or a 64 bit integer.</param>
        /// <returns>The read integer.</returns>
        public static ulong ReadNativeInt(this IBinaryStreamReader reader, bool is32Bit)
        {
            return is32Bit ? reader.ReadUInt32() : reader.ReadUInt64();
        }


        /// <summary>
        /// Reads a compressed unsigned integer from the stream.
        /// </summary>
        /// <param name="reader">The reader to use for reading the data.</param>
        /// <returns>The unsigned integer that was read from the stream.</returns>
        public static uint ReadCompressedUInt32(this IBinaryStreamReader reader)
        {
            var firstByte = reader.ReadByte();

            if ((firstByte & 0x80) == 0)
                return firstByte;

            if ((firstByte & 0x40) == 0)
                return (uint)(((firstByte & 0x7F) << 8) | reader.ReadByte());

            return (uint)(((firstByte & 0x3F) << 0x18) |
                          (reader.ReadByte() << 0x10) |
                          (reader.ReadByte() << 0x08) |
                          reader.ReadByte());
        }

        /// <summary>
        /// Tries to reads a compressed unsigned integer from the stream.
        /// </summary>
        /// <param name="reader">The reader to use for reading the data.</param>
        /// <param name="value">The unsigned integer that was read from the stream.</param>
        /// <returns><c>True</c> if the method succeeded, false otherwise.</returns>
        public static bool TryReadCompressedUInt32(this IBinaryStreamReader reader, out uint value)
        {
            value = 0;
            if (!reader.CanRead(sizeof(byte)))
                return false;

            var firstByte = reader.ReadByte();
            reader.FileOffset--;

            if (((firstByte & 0x80) == 0 && reader.CanRead(sizeof(byte))) ||
                ((firstByte & 0x40) == 0 && reader.CanRead(sizeof(ushort))) ||
                (reader.CanRead(sizeof(uint))))
            {
                value = ReadCompressedUInt32(reader);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reads a short or a long index from the stream.
        /// </summary>
        /// <param name="reader">The reader to use for reading the data.</param>
        /// <param name="size">The size of the index to read.</param>
        /// <returns>The index, zero extended to 32 bits if necessary.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static uint ReadIndex(this IBinaryStreamReader reader, IndexSize size)
        {
            switch (size)
            {
                case IndexSize.Short:
                    return reader.ReadUInt16();
                case IndexSize.Long:
                    return reader.ReadUInt32();
                default:
                    throw new ArgumentOutOfRangeException(nameof(size));
            }
        }
        
        /// <summary>
        /// Reads a serialized UTF8 string from the stream.
        /// </summary>
        /// <param name="reader">The reader to use for reading the data.</param>
        /// <returns>The string that was read from the stream.</returns>
        public static string ReadSerString(this IBinaryStreamReader reader)
        {
            if (!reader.CanRead(1) || reader.ReadByte() == 0xFF)
                return null;
            reader.FileOffset--;
            if (!reader.TryReadCompressedUInt32(out uint length))
                return null;
            var data = new byte[length];
            length = (uint) reader.ReadBytes(data, 0, (int) length);
            return Encoding.UTF8.GetString(data, 0, (int) length);
        }
    }
}
