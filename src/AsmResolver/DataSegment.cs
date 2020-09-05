using System;
using System.IO;

namespace AsmResolver
{
    /// <summary>
    /// Provides an implementation of a segment using a byte array to represent its contents. 
    /// </summary>
    public class DataSegment : SegmentBase, IReadableSegment
    {
        /// <summary>
        /// Puts the remaining data of the provided input stream into a new data segment. 
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The data segment containing the remaining data.</returns>
        public static DataSegment FromReader(IBinaryStreamReader reader)
        {
            return FromReader(reader, (int) (reader.StartOffset + reader.Length - reader.Offset));
        }
        
        /// <summary>
        /// Reads a single data segment at the current position of the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The read data segment.</returns>
        public static DataSegment FromReader(IBinaryStreamReader reader, int count)
        {
            ulong position = reader.Offset;
            uint rva = reader.Rva;
            
            var buffer = new byte[count];
            reader.ReadBytes(buffer, 0, count);
            
            return new DataSegment(buffer)
            {
                Offset = position,
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
        
        /// <summary>
        /// Gets the data that is stored in the segment.
        /// </summary>
        public byte[] Data
        {
            get;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => (uint) Data.Length;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteBytes(Data, 0, Data.Length);
        }

        /// <inheritdoc />
        public IBinaryStreamReader CreateReader(ulong fileOffset, uint size)
        {
            if (fileOffset < Offset || fileOffset > Offset + (ulong) Data.Length)
                throw new ArgumentOutOfRangeException(nameof(fileOffset));
            if (fileOffset + size > Offset + (ulong) Data.Length)
                throw new EndOfStreamException();

            return new ByteArrayReader(Data,
                (uint) (fileOffset - Offset),
                size,
                fileOffset,
                (uint) (fileOffset - Offset + Rva));
        }
        
    }
}