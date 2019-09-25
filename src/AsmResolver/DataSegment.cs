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

using System;
using System.IO;

namespace AsmResolver
{
    /// <summary>
    /// Provides an implementation of a segment using a byte array to represent its contents. 
    /// </summary>
    public class DataSegment : IReadableSegment
    {
        /// <summary>
        /// Puts the remaining data of the provided input stream into a new data segment. 
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The data segment containing the remaining data.</returns>
        public static DataSegment FromReader(IBinaryStreamReader reader)
        {
            return FromReader(reader, (int) (reader.StartPosition + reader.Length - reader.FileOffset));
        }
        
        /// <summary>
        /// Reads a single data segment at the current position of the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The read data segment.</returns>
        public static DataSegment FromReader(IBinaryStreamReader reader, int count)
        {
            uint position = reader.FileOffset;
            uint rva = reader.Rva;
            
            var buffer = new byte[count];
            reader.ReadBytes(buffer, 0, count);
            
            return new DataSegment(buffer)
            {
                FileOffset = position,
                Rva = rva
            };
        }
        
        /// <summary>
        /// Creates a new data segment using the provided byte array as contents.
        /// </summary>
        /// <param name="data">The data to store.</param>
        public DataSegment(byte[] data)
        {
            Data = data;
        }

        /// <inheritdoc />
        public uint FileOffset
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public uint Rva
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the data that is stored in the segment.
        /// </summary>
        public byte[] Data
        {
            get;
        }

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            FileOffset = newFileOffset;
            Rva = newRva;
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            return (uint) Data.Length;
        }

        /// <inheritdoc />
        public uint GetVirtualSize()
        {
            return (uint) Data.Length;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            writer.WriteBytes(Data, 0, Data.Length);
        }

        /// <inheritdoc />
        public IBinaryStreamReader CreateReader(uint fileOffset, uint size)
        {
            if (fileOffset < FileOffset || fileOffset > FileOffset + Data.Length)
                throw new ArgumentOutOfRangeException(nameof(fileOffset));
            if (fileOffset + size > FileOffset + Data.Length)
                throw new EndOfStreamException();

            return new ByteArrayReader(Data,
                (int) (fileOffset - FileOffset),
                size,
                FileOffset,
                fileOffset + Rva);
        }
        
    }
}