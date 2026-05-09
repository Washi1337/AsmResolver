using System;
using System.IO;

namespace AsmResolver.IO;

public struct BinaryStreamReaderState : IEquatable<BinaryStreamReaderState>
{
    /// <summary>
    /// Creates a new binary stream reader on the provided data source.
    /// </summary>
    /// <param name="data">The data to read from.</param>
    public BinaryStreamReaderState(byte[] data)
        : this(new ByteArrayDataSource(data))
    {
    }

    /// <summary>
    /// Creates a new binary stream reader on the provided data source.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public BinaryStreamReaderState(Stream stream)
        : this(new StreamDataSource(stream))
    {
    }

    /// <summary>
    /// Creates a new binary stream reader on the provided data source.
    /// </summary>
    /// <param name="dataSource">The object to get the data from.</param>
    public BinaryStreamReaderState(IDataSource dataSource)
        : this(dataSource, dataSource.BaseAddress, 0, (uint) dataSource.Length, dataSource.BaseAddress)
    {
    }

    public BinaryStreamReaderState(
        IDataSource dataSource,
        ulong startOffset,
        uint startRva,
        uint length,
        ulong offset)
    {
        if (dataSource is null)
            throw new ArgumentNullException(nameof(dataSource));

        if (length > 0)
        {
            if (!dataSource.IsValidAddress(offset))
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (!dataSource.IsValidAddress(offset + length - 1))
                throw new EndOfStreamException("Offset and address reach outside of the boundaries of the data source.");
        }

        DataSource = dataSource;
        StartOffset = startOffset;
        StartRva = startRva;
        Length = length;
        CurrentOffset = offset;
    }

    public IDataSource DataSource { get; set; }
    public ulong StartOffset { get; set; }
    public uint StartRva { get; set; }
    public uint Length { get; set; }
    public ulong CurrentOffset { get; set; }

    public readonly BinaryStreamReader CreateReader() => new(this);

    public readonly BinaryStreamReaderState WithOffset(ulong offset)
    {
        return WithOffsetSize(offset, (uint) (Length - (offset - StartOffset)));
    }

    public readonly BinaryStreamReaderState WithOffsetSize(ulong offset, uint size)
    {
        return new BinaryStreamReaderState(
            dataSource: DataSource,
            startOffset: offset,
            startRva: (uint) (StartRva + (offset - StartOffset)),
            length: size,
            offset: offset
        );
    }

    public readonly BinaryStreamReaderState WithRelativeOffset(uint relativeOffset)
    {
        return WithRelativeOffsetSize(relativeOffset, Length - relativeOffset);
    }

    public readonly BinaryStreamReaderState WithRelativeOffsetSize(ulong offset, uint size)
    {
        return new BinaryStreamReaderState(
            dataSource: DataSource,
            startOffset: StartOffset + offset,
            startRva: (uint) (StartRva + offset),
            length: size,
            offset: StartOffset + offset
        );
    }

    public bool Equals(BinaryStreamReaderState other)
    {
        return DataSource.Equals(other.DataSource)
            && StartOffset == other.StartOffset
            && StartRva == other.StartRva
            && Length == other.Length
            && CurrentOffset == other.CurrentOffset;
    }

    public override bool Equals(object? obj)
    {
        return obj is BinaryStreamReaderState other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = DataSource.GetHashCode();
            hashCode = (hashCode * 397) ^ StartOffset.GetHashCode();
            hashCode = (hashCode * 397) ^ (int) StartRva;
            hashCode = (hashCode * 397) ^ (int) Length;
            hashCode = (hashCode * 397) ^ CurrentOffset.GetHashCode();
            return hashCode;
        }
    }
}
