using System;
using System.Text;
using AsmResolver.Net;

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
        long StartPosition
        {
            get;
        }

        /// <summary>
        /// Gets the current position of the reader.
        /// </summary>
        long Position
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the maximum length of data the reader can read from the stream.
        /// </summary>
        long Length
        {
            get;
        }

        /// <summary>
        /// Forks the reader by creating a new instance of a binary stream reader, using the same data source, but a different address and size.
        /// </summary>
        /// <param name="address">The address of the forked reader to start at.</param>
        /// <param name="size">The size of the data the forked reader can read.</param>
        /// <returns>A forked binary stream reader with the same data source, but a different address and size.</returns>
        IBinaryStreamReader CreateSubReader(long address, int size);

        /// <summary>
        /// Reads data from the stream until a specific byte is encountered.
        /// </summary>
        /// <param name="value">The byte to use as a delimeter.</param>
        /// <returns>The data that was read from the stream, including the delimeter.</returns>
        byte[] ReadBytesUntil(byte value);

        /// <summary>
        /// Reads a specific amount of bytes from the stream.
        /// </summary>
        /// <param name="count">The amount of bytes to read.</param>
        /// <returns>The data that was read from the stream.</returns>
        byte[] ReadBytes(int count);

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

    public static class BinaryStreamReaderExtensions
    {
        /// <summary>
        /// Determines whether the reader can read up to a specific amount of bytes.
        /// </summary>
        /// <param name="reader">The reader to check.</param>
        /// <param name="size">The size of the data to check.</param>
        /// <returns></returns>
        public static bool CanRead(this IBinaryStreamReader reader, int size)
        {
            return (reader.Position - reader.StartPosition) + size <= reader.Length;
        }

        /// <summary>
        /// Forks the reader by creating a new instance of a binary stream reader, using the same data source, but a different address.
        /// </summary>
        /// <param name="reader">The reader to fork.</param>
        /// <param name="address">The address of the forked reader to start at.</param>
        /// <returns>A forked binary stream reader with the same data source, but a different address.</returns>
        public static IBinaryStreamReader CreateSubReader(this IBinaryStreamReader reader, long address)
        {
            return reader.CreateSubReader(address, (int)(reader.Length - (address - reader.StartPosition)));
        }

        /// <summary>
        /// Reads a zero-terminated ASCII string from the stream.
        /// </summary>
        /// <param name="reader">The reader to use for reading the data.</param>
        /// <returns>The string that was read from the stream.</returns>
        public static string ReadAsciiString(this IBinaryStreamReader reader)
        {
            return Encoding.ASCII.GetString(reader.ReadBytesUntil(0));
        }

        /// <summary>
        /// Reads an aligned ASCII string from the stream.
        /// </summary>
        /// <param name="reader">The reader to use for reading the data.</param>
        /// <param name="align">The alignment to use.</param>
        /// <returns>The string that was read from the stream.</returns>
        public static string ReadAlignedAsciiString(this IBinaryStreamReader reader, int align)
        {
            var position = reader.Position;
            var value = reader.ReadAsciiString();
            do
            {
                reader.Position++;
            } while ((reader.Position - position) % align != 0);
            return value;
        }

        /// <summary>
        /// Reads a serialized UTF8 string from the steram.
        /// </summary>
        /// <param name="reader">The reader to use for reading the data.</param>
        /// <returns>The string that was read from the stream.</returns>
        public static string ReadSerString(this IBinaryStreamReader reader)
        {
            if (reader.ReadByte() == 0xFF)
                return null;
            reader.Position--;
            uint length;
            if (!reader.TryReadCompressedUInt32(out length))
                return null;
            return Encoding.UTF8.GetString(reader.ReadBytes((int)length));
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
            reader.Position--;

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
        /// Reads an index with the specified index size of the stream.
        /// </summary>
        /// <param name="reader">The reader to use for reading the data.</param>
        /// <param name="size">The size of the index.</param>
        /// <returns>The index.</returns>
        public static uint ReadIndex(this IBinaryStreamReader reader, IndexSize size)
        {
            switch (size)
            {
                case IndexSize.Short:
                    return reader.ReadUInt16();
                case IndexSize.Long:
                    return reader.ReadUInt32();
                default:
                    throw new ArgumentOutOfRangeException("size");
            }
        }

        /// <summary>
        /// Aligns the reader to a specified boundary.
        /// </summary>
        /// <param name="reader">The reader to align.</param>
        /// <param name="align">The boundary to use.</param>
        public static void Align(this IBinaryStreamReader reader, int align)
        {
            align--;
            reader.ReadBytes((((int)reader.Position + align) & ~align) - (int)reader.Position);
        }
    }
}
