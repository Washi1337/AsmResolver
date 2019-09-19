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
        
    }
}
