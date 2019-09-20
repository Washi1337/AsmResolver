// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

using System.Text;

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
        uint FileOffset
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

    }

    public static partial class Extensions
    {
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
        /// Aligns the writer to a specified boundary.
        /// </summary>
        /// <param name="writer">The writer to align.</param>
        /// <param name="align">The boundary to use.</param>
        public static void Align(this IBinaryStreamWriter writer, uint align)
        {
            uint currentPosition = writer.FileOffset;
            writer.WriteZeroes((int) (currentPosition.Align(align) - writer.FileOffset));
        }
        
    }
}
