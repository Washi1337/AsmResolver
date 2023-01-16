using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides methods for reading binary data from a data source.
    /// </summary>
    public struct BinaryStreamReader
    {
        [ThreadStatic]
        private static int[]? _buffer;

        /// <summary>
        /// Creates a new binary stream reader on the provided data source.
        /// </summary>
        /// <param name="data">The data to read from.</param>
        public BinaryStreamReader(byte[] data)
            : this(new ByteArrayDataSource(data))
        {
        }

        /// <summary>
        /// Creates a new binary stream reader on the provided data source.
        /// </summary>
        /// <param name="dataSource">The object to get the data from.</param>
        public BinaryStreamReader(IDataSource dataSource)
            : this(dataSource, 0, 0, (uint) dataSource.Length)
        {
        }

        /// <summary>
        /// Creates a new binary stream reader on the provided data source.
        /// </summary>
        /// <param name="dataSource">The object to get the data from.</param>
        /// <param name="offset">The raw offset to start at.</param>
        /// <param name="rva">The relative virtual address associated to the offset.</param>
        /// <param name="length">The maximum number of bytes to read.</param>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="dataSource"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when <paramref name="offset"/> is not a valid offset.</exception>
        /// <exception cref="EndOfStreamException">Occurs when too many bytes are specified by <paramref name="length"/>.</exception>
        public BinaryStreamReader(IDataSource dataSource, ulong offset, uint rva, uint length)
        {
            if (dataSource is null)
                throw new ArgumentNullException(nameof(dataSource));

            if (length > 0)
            {
                if (!dataSource.IsValidAddress(offset))
                    throw new ArgumentOutOfRangeException(nameof(offset));

                if (!dataSource.IsValidAddress(offset + length - 1))
                {
                    throw new EndOfStreamException(
                        "Offset and address reach outside of the boundaries of the data source.");
                }
            }

            DataSource = dataSource;
            StartOffset = Offset = offset;
            StartRva = rva;
            Length = length;
        }

        /// <summary>
        /// Gets the data source the reader is reading from.
        /// </summary>
        public IDataSource DataSource
        {
            get;
        }

        /// <summary>
        /// Gets the raw offset this reader started from.
        /// </summary>
        public ulong StartOffset
        {
            get;
        }

        /// <summary>
        /// Gets the relative virtual address this reader started from.
        /// </summary>
        public uint StartRva
        {
            get;
        }

        /// <summary>
        /// Gets the number of bytes that can be read by the reader.
        /// </summary>
        public uint Length
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the raw address indicating the end of the stream.
        /// </summary>
        public ulong EndOffset => StartOffset + Length;

        /// <summary>
        /// Gets the relative virtual address indicating the end of the stream.
        /// </summary>
        public ulong EndRva => StartRva + Length;

        /// <summary>
        /// Gets or sets the current raw offset to read from.
        /// </summary>
        public ulong Offset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current offset relative to the beginning of <see cref="StartOffset"/> to read from.
        /// </summary>
        public uint RelativeOffset
        {
            get => (uint) (Offset - StartOffset);
            set => Offset = value + StartOffset;
        }

        /// <summary>
        /// Gets or sets the current virtual address (relative to the image base) to read from.
        /// </summary>
        public uint Rva
        {
            get => RelativeOffset + StartRva;
            set => RelativeOffset = value - StartRva;
        }

        /// <summary>
        /// Gets a value indicating whether the reader is in a valid state.
        /// </summary>
        public bool IsValid => DataSource is not null;

        /// <summary>
        /// Determines whether the provided number of bytes can be read from the current position.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns><c>true</c> if the provided number of byte can be read, <c>false</c> otherwise.</returns>
        public bool CanRead(uint count) => RelativeOffset + count <= Length;

        private void AssertCanRead(uint count)
        {
            if (!CanRead(count))
                throw new EndOfStreamException();
        }

        /// <summary>
        /// Peeks a single byte from the input stream.
        /// </summary>
        /// <returns>The read byte, or <c>-1</c> if no byte could be read.</returns>
        public int PeekByte() => CanRead(1)
            ? DataSource[Offset]
            : -1;

        /// <summary>
        /// Reads a single byte from the input stream, and advances the current offset by one.
        /// </summary>
        /// <returns>The consumed value.</returns>
        public byte ReadByte()
        {
            AssertCanRead(sizeof(byte));
            return DataSource[Offset++];
        }

        /// <summary>
        /// Reads a single unsigned 16-bit integer from the input stream, and advances the current offset by two.
        /// </summary>
        /// <returns>The consumed value.</returns>
        public ushort ReadUInt16()
        {
            AssertCanRead(2);
            ushort value = (ushort) (DataSource[Offset]
                                     | (DataSource[Offset + 1] << 8));
            Offset += 2;
            return value;
        }

        /// <summary>
        /// Reads a single unsigned 32-bit integer from the input stream, and advances the current offset by four.
        /// </summary>
        /// <returns>The consumed value.</returns>
        public uint ReadUInt32()
        {
            AssertCanRead(4);
            uint value = unchecked((uint) (DataSource[Offset]
                                           | (DataSource[Offset + 1] << 8)
                                           | (DataSource[Offset + 2] << 16)
                                           | (DataSource[Offset + 3] << 24)));
            Offset += 4;
            return value;
        }

        /// <summary>
        /// Reads a single unsigned 64-bit integer from the input stream, and advances the current offset by eight.
        /// </summary>
        /// <returns>The consumed value.</returns>
        public ulong ReadUInt64()
        {
            AssertCanRead(8);
            ulong value = unchecked((ulong) (DataSource[Offset]
                                             | ( (long) DataSource[Offset + 1] << 8)
                                             | ( (long) DataSource[Offset + 2] << 16)
                                             | ( (long) DataSource[Offset + 3] << 24)
                                             | ( (long) DataSource[Offset + 4] << 32)
                                             | ( (long) DataSource[Offset + 5] << 40)
                                             | ( (long) DataSource[Offset + 6] << 48)
                                             | ( (long) DataSource[Offset + 7] << 56)));
            Offset += 8;
            return value;
        }

        /// <summary>
        /// Reads a single signed byte from the input stream, and advances the current offset by one.
        /// </summary>
        /// <returns>The consumed value.</returns>
        public sbyte ReadSByte()
        {
            AssertCanRead(1);
            return unchecked((sbyte) DataSource[Offset++]);
        }

        /// <summary>
        /// Reads a single signed 16-bit integer from the input stream, and advances the current offset by two.
        /// </summary>
        /// <returns>The consumed value.</returns>
        public short ReadInt16()
        {
            AssertCanRead(2);
            short value = (short) (DataSource[Offset]
                                   | (DataSource[Offset + 1] << 8));
            Offset += 2;
            return value;
        }

        /// <summary>
        /// Reads a single signed 32-bit integer from the input stream, and advances the current offset by four.
        /// </summary>
        /// <returns>The consumed value.</returns>
        public int ReadInt32()
        {
            AssertCanRead(4);
            int value = DataSource[Offset]
                        | (DataSource[Offset + 1] << 8)
                        | (DataSource[Offset + 2] << 16)
                        | (DataSource[Offset + 3] << 24);
            Offset += 4;
            return value;
        }

        /// <summary>
        /// Reads a single signed 64-bit integer from the input stream, and advances the current offset by eight.
        /// </summary>
        /// <returns>The consumed value.</returns>
        public long ReadInt64()
        {
            AssertCanRead(8);
            long value = DataSource[Offset]
                         | ((long) DataSource[Offset + 1] << 8)
                         | ((long) DataSource[Offset + 2] << 16)
                         | ((long) DataSource[Offset + 3] << 24)
                         | ((long) DataSource[Offset + 4] << 32)
                         | ((long) DataSource[Offset + 5] << 40)
                         | ((long) DataSource[Offset + 6] << 48)
                         | ((long) DataSource[Offset + 7] << 56);
            Offset += 8;
            return value;
        }

        /// <summary>
        /// Reads a single signed 32-bit single precision floating point number from the input stream, and advances the
        /// current offset by four.
        /// </summary>
        /// <returns>The consumed value.</returns>
        public unsafe float ReadSingle()
        {
            uint raw = ReadUInt32();
            return *(float*) &raw;
        }

        /// <summary>
        /// Reads a single signed 64-bit double precision floating point number from the input stream, and advances the
        /// current offset by four.
        /// </summary>
        /// <returns>The consumed value.</returns>
        public unsafe double ReadDouble()
        {
            ulong raw = ReadUInt64();
            return *(double*) &raw;
        }

        /// <summary>
        /// Reads a single 128-bit decimal value from the input stream, and advances the current offset by 16.
        /// </summary>
        /// <returns>The consumed value.</returns>
        public decimal ReadDecimal()
        {
            AssertCanRead(4 * sizeof(int));

            _buffer ??= new int[4];
            for (int i = 0; i < 4; i++)
                _buffer[i] = ReadInt32();

            return new decimal(_buffer);
        }

        /// <summary>
        /// Attempts to read the provided amount of bytes from the input stream.
        /// </summary>
        /// <param name="buffer">The buffer that receives the read bytes.</param>
        /// <param name="index">The index into the buffer to start writing into.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes that were read.</returns>
        public int ReadBytes(byte[] buffer, int index, int count)
        {
            int actualLength = DataSource.ReadBytes(Offset, buffer, index, count);
            Offset += (uint) actualLength;
            return actualLength;
        }

        /// <summary>
        /// Creates a segment containing data from the input data source, starting at the current position of the input
        /// stream. The reader then advances the current offset by the provided number of bytes.
        /// </summary>
        /// <param name="count">The number of bytes the segment should contain.</param>
        /// <returns>The read segment.</returns>
        public IReadableSegment ReadSegment(uint count)
        {
            var segment = new DataSourceSegment(DataSource, Offset, Rva, count);
            Offset += count;
            return segment;
        }

        /// <summary>
        /// Consumes the remainder of the input stream.
        /// </summary>
        /// <returns>The remaining bytes.</returns>
        public byte[] ReadToEnd()
        {
            byte[] buffer = new byte[Length - RelativeOffset];
            ReadBytes(buffer, 0, buffer.Length);
            return buffer;
        }

        /// <summary>
        /// Reads bytes from the input stream until the provided delimiter byte is reached.
        /// </summary>
        /// <param name="delimiter">The delimiter byte to stop at.</param>
        /// <returns>The read bytes, including the delimiter if it was found.</returns>
        public byte[] ReadBytesUntil(byte delimiter) => ReadBytesUntil(delimiter, true);

        /// <summary>
        /// Reads bytes from the input stream until the provided delimiter byte is reached.
        /// </summary>
        /// <param name="delimiter">The delimiter byte to stop at.</param>
        /// <param name="includeDelimiterInReturn">
        /// <c>true</c> if the final delimiter should be included in the return value, <c>false</c> otherwise.
        /// </param>
        /// <returns>The read bytes.</returns>
        /// <remarks>
        /// This function always consumes the delimiter from the input stream if it is present, regardless of the value
        /// of <paramref name="includeDelimiterInReturn"/>.
        /// </remarks>
        public byte[] ReadBytesUntil(byte delimiter, bool includeDelimiterInReturn)
        {
            var lookahead = Fork();
            bool hasConsumedDelimiter = lookahead.AdvanceUntil(delimiter, includeDelimiterInReturn);

            byte[] buffer = new byte[lookahead.RelativeOffset - RelativeOffset];
            ReadBytes(buffer, 0, buffer.Length);

            if (hasConsumedDelimiter)
                ReadByte();

            return buffer;
        }

        /// <summary>
        /// Advances the reader until the provided delimiter byte is reached.
        /// </summary>
        /// <param name="delimiter">The delimiter byte to stop at.</param>
        /// <param name="consumeDelimiter">
        /// <c>true</c> if the final delimiter should be consumed if available, <c>false</c> otherwise.
        /// </param>
        /// <returns><c>true</c> if the delimiter byte was found and consumed, <c>false</c> otherwise.</returns>
        public bool AdvanceUntil(byte delimiter, bool consumeDelimiter)
        {
            while (RelativeOffset < Length)
            {
                byte b = ReadByte();
                if (b == delimiter)
                {
                    if (!consumeDelimiter)
                    {
                        RelativeOffset--;
                        return true;
                    }

                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Reads a null-terminated ASCII string from the input stream.
        /// </summary>
        /// <returns>The read ASCII string, excluding the null terminator.</returns>
        public string ReadAsciiString() => Encoding.ASCII.GetString(ReadBytesUntil(0, false));

        /// <summary>
        /// Reads a zero-terminated Unicode string from the stream.
        /// </summary>
        /// <returns>The string that was read from the stream.</returns>
        public string ReadUnicodeString()
        {
            var builder = new StringBuilder();

            while (true)
            {
                char nextChar = (char) ReadUInt16();
                if (nextChar is '\0')
                    break;
                builder.Append(nextChar);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Reads a null-terminated UTF-8 string from the input stream.
        /// </summary>
        /// <returns>The read UTF-8 string, excluding the null terminator.</returns>
        public Utf8String ReadUtf8String()
        {
            byte[] data = ReadBytesUntil(0, false);
            return data.Length != 0
                ? new Utf8String(data)
                : Utf8String.Empty;
        }

        /// <summary>
        /// Reads either a 32-bit or a 64-bit number from the input stream.
        /// </summary>
        /// <param name="is32Bit">Indicates the integer to be read is 32-bit or 64-bit.</param>
        /// <returns>The read number, zero extended if necessary.</returns>
        public ulong ReadNativeInt(bool is32Bit) => is32Bit ? ReadUInt32() : ReadUInt64();

        /// <summary>
        /// Reads a compressed unsigned integer from the stream.
        /// </summary>
        /// <returns>The unsigned integer that was read from the stream.</returns>
        public uint ReadCompressedUInt32()
        {
            byte firstByte = ReadByte();

            if ((firstByte & 0x80) == 0)
                return firstByte;

            if ((firstByte & 0x40) == 0)
                return (uint)(((firstByte & 0x7F) << 8) | ReadByte());

            return (uint) (((firstByte & 0x3F) << 0x18) |
                           (ReadByte() << 0x10) |
                           (ReadByte() << 0x08) |
                           ReadByte());
        }

        /// <summary>
        /// Tries to reads a compressed unsigned integer from the stream.
        /// </summary>
        /// <param name="value">The unsigned integer that was read from the stream.</param>
        /// <returns><c>True</c> if the method succeeded, false otherwise.</returns>
        public bool TryReadCompressedUInt32(out uint value)
        {
            value = 0;
            if (!CanRead(sizeof(byte)))
                return false;

            byte firstByte = ReadByte();
            Offset--;

            if ((firstByte & 0x80) == 0 && CanRead(sizeof(byte)) ||
                (firstByte & 0x40) == 0 && CanRead(sizeof(ushort)) ||
                CanRead(sizeof(uint)))
            {
                value = ReadCompressedUInt32();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reads a 7-bit encoded 32-bit integer from the stream.
        /// </summary>
        /// <returns>The integer.</returns>
        /// <exception cref="FormatException">Occurs when an invalid 7-bit encoding was encountered.</exception>
        public int Read7BitEncodedInt32()
        {
            int result = 0;
            byte currentByte;

            for (int i = 0; i < 4; i++)
            {
                currentByte = ReadByte();
                result |= (currentByte & 0x7F) << (i * 7);

                if ((currentByte & 0x80) == 0)
                    return result;
            }

            currentByte = ReadByte();
            if (currentByte > 0b11111)
                throw new FormatException("Invalid 7-bit encoded integer.");

            return result | (currentByte << (4 * 7));
        }

        /// <summary>
        /// Reads a short or a long index from the stream.
        /// </summary>
        /// <param name="size">The size of the index to read.</param>
        /// <returns>The index, zero extended to 32 bits if necessary.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public uint ReadIndex(IndexSize size)
        {
            switch (size)
            {
                case IndexSize.Short:
                    return ReadUInt16();
                case IndexSize.Long:
                    return ReadUInt32();
                default:
                    throw new ArgumentOutOfRangeException(nameof(size));
            }
        }

        /// <summary>
        /// Reads a serialized UTF8 string from the stream.
        /// </summary>
        /// <returns>The string that was read from the stream.</returns>
        public Utf8String? ReadSerString()
        {
            if (!CanRead(1) || DataSource[Offset] == 0xFF)
            {
                Offset++;
                return null;
            }

            if (!TryReadCompressedUInt32(out uint length))
                return null;

            byte[] data = new byte[length];
            length = (uint) ReadBytes(data, 0, (int) length);
            return new Utf8String(data, 0, (int)length);
        }

        /// <summary>
        /// Reads a serialized UTF-8 string that is prefixed by a 7-bit encoded length header.
        /// </summary>
        /// <returns>The string.</returns>
        /// <exception cref="FormatException">Occurs when the 7-bit encoded header is invalid.</exception>
        public string ReadBinaryFormatterString() => ReadBinaryFormatterString(Encoding.UTF8);

        /// <summary>
        /// Reads a serialized string that is prefixed by a 7-bit encoded length header.
        /// </summary>
        /// <param name="encoding">The encoding to use for decoding the bytes into a string.</param>
        /// <returns>The string.</returns>
        /// <exception cref="FormatException">Occurs when the 7-bit encoded header is invalid.</exception>
        public string ReadBinaryFormatterString(Encoding encoding)
        {
            int length = Read7BitEncodedInt32();

            switch (length)
            {
                case < 0:
                    throw new FormatException("Negative string length.");

                case 0:
                    return string.Empty;

                case > 0:
                    byte[] bytes = new byte[length];
                    int actualLength = ReadBytes(bytes, 0, length);
                    return encoding.GetString(bytes, 0, actualLength);
            }
        }

        /// <summary>
        /// Aligns the reader to a specified boundary.
        /// </summary>
        /// <param name="alignment">The boundary to use.</param>
        public void Align(uint alignment) => Offset = Offset.Align(alignment);

        /// <summary>
        /// Aligns the reader to a specified boundary.
        /// </summary>
        /// <param name="alignment">The boundary to use.</param>
        public void AlignRelative(uint alignment) => RelativeOffset = RelativeOffset.Align(alignment);

        /// <summary>
        /// Creates an exact copy of the reader.
        /// </summary>
        /// <returns>The copied reader.</returns>
        /// <remarks>This method does not copy the underlying buffer.</remarks>
        public readonly BinaryStreamReader Fork() => this;

        /// <summary>
        /// Creates a copy of the reader, and moves the offset of the copied reader to the provided file offset.
        /// </summary>
        /// <param name="offset">The file offset.</param>
        /// <returns>The new reader.</returns>
        /// <remarks>This method does not copy the underlying buffer.</remarks>
        public readonly BinaryStreamReader ForkAbsolute(ulong offset)
        {
            return ForkAbsolute(offset, (uint) (Length - (offset - StartOffset)));
        }

        /// <summary>
        /// Creates a copy of the reader, moves the offset of the copied reader to the provided file offset, and resizes
        /// the copied reader to the provided number of bytes.
        /// </summary>
        /// <param name="offset">The file offset.</param>
        /// <param name="size">The number of bytes to read.</param>
        /// <returns>The new reader.</returns>
        /// <remarks>This method does not copy the underlying buffer.</remarks>
        public readonly BinaryStreamReader ForkAbsolute(ulong offset, uint size)
        {
            return new(DataSource, offset, (uint) (StartRva + (offset - StartOffset)), size);
        }

        /// <summary>
        /// Creates a copy of the reader, and moves to the provided relative offset of the copied reader.
        /// </summary>
        /// <param name="relativeOffset">The displacement.</param>
        /// <returns>The new reader.</returns>
        /// <remarks>This method does not copy the underlying buffer.</remarks>
        public readonly BinaryStreamReader ForkRelative(uint relativeOffset)
        {
            return ForkRelative(relativeOffset, Length - relativeOffset);
        }

        /// <summary>
        /// Creates a copy of the reader, moves the copied reader to the provided relative offset, and resizes the
        /// copied reader to the provided number of bytes.
        /// </summary>
        /// <param name="relativeOffset">The displacement.</param>
        /// <param name="size">The number of bytes to read.</param>
        /// <returns>The new reader.</returns>
        /// <remarks>This method does not copy the underlying buffer.</remarks>
        public readonly BinaryStreamReader ForkRelative(uint relativeOffset, uint size)
        {
            return new(DataSource, StartOffset + relativeOffset, StartRva + relativeOffset, size);
        }

        /// <summary>
        /// Resizes the current reader to a new number of bytes.
        /// </summary>
        /// <param name="newSize">The new number of bytes.</param>
        /// <exception cref="EndOfStreamException">
        /// Occurs when the provided size reaches outside of the input stream's length.
        /// </exception>
        public void ChangeSize(uint newSize)
        {
            if (newSize > Length)
                throw new EndOfStreamException();

            Length = newSize;
        }

        /// <summary>
        /// Consumes and copies the remainder of the contents to the provided output stream.
        /// </summary>
        /// <param name="writer">The output stream.</param>
        public void WriteToOutput(IBinaryStreamWriter writer)
        {
            byte[] buffer = new byte[4096];
            while (RelativeOffset < Length)
            {
                int blockSize = (int) Math.Min(buffer.Length, Length - RelativeOffset);
                int actualSize = ReadBytes(buffer, 0, blockSize);
                if (actualSize == 0)
                {
                    writer.WriteZeroes((int) (Length - RelativeOffset));
                    return;
                }

                writer.WriteBytes(buffer, 0, actualSize);
            }
        }
    }
}
