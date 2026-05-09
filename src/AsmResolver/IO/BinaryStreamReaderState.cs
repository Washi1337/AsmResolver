using System;
using System.IO;

namespace AsmResolver.IO;

/// <summary>
/// Provides the stateful information for a <see cref="BinaryStreamReader"/>.
/// </summary>
public struct BinaryStreamReaderState : IEquatable<BinaryStreamReaderState>
{
    /// <summary>
    /// Creates a new binary stream reader state on the provided data source.
    /// </summary>
    /// <param name="data">The data to read from.</param>
    public BinaryStreamReaderState(byte[] data)
        : this(new ByteArrayDataSource(data))
    {
    }

    /// <summary>
    /// Creates a new binary stream reader state on the provided data source.
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

    /// <summary>
    /// Creates a new binary stream reader on the provided data source.
    /// </summary>
    /// <param name="dataSource">The object to get the data from.</param>
    /// <param name="startOffset">The start offset</param>
    /// <param name="startRva">The start rva</param>
    /// <param name="length">The length of the buffer</param>
    /// <param name="offset">The current offset within the buffer</param>
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

    /// <summary>
    /// Gets or sets the object the reader pulls data from.
    /// </summary>
    public IDataSource DataSource { get; set; }

    /// <summary>
    /// Gets or sets the start offset of the buffer within the data source.
    /// </summary>
    public ulong StartOffset { get; set; }

    /// <summary>
    /// Gets or sets the start rva of the buffer within the data source.
    /// </summary>
    public uint StartRva { get; set; }

    /// <summary>
    /// Gets or sets the length of the buffer within the data source.
    /// </summary>
    public uint Length { get; set; }

    /// <summary>
    /// Gets or sets the current reader offset.
    /// </summary>
    public ulong CurrentOffset
    {
        get => StartOffset + RelativeOffset;
        set => RelativeOffset = (uint) (value - StartOffset);
    }

    /// <summary>
    /// Gets or sets the current index relative to the start of the buffer.
    /// </summary>
    public uint RelativeOffset { get; set; }

    /// <summary>
    /// Creates a reader based on the current reader state.
    /// </summary>
    /// <returns>The reader.</returns>
    public readonly BinaryStreamReader CreateReader() => new(this);

    /// <summary>
    /// Creates a new reader state with the provided offset.
    /// </summary>
    /// <param name="offset">The offset.</param>
    /// <returns>The new state.</returns>
    public readonly BinaryStreamReaderState WithOffset(ulong offset)
    {
        return WithOffsetSize(offset, (uint) (Length - (offset - StartOffset)));
    }

    /// <summary>
    /// Creates a new reader state with the provided offset.
    /// </summary>
    /// <param name="offset">The offset.</param>
    /// <param name="size">The size.</param>
    /// <returns>The new state.</returns>
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

    /// <summary>
    /// Creates a new reader state with the provided offset relative to the current start offset.
    /// </summary>
    /// <param name="relativeOffset">The offset.</param>
    /// <returns>The new state.</returns>
    public readonly BinaryStreamReaderState WithRelativeOffset(uint relativeOffset)
    {
        return WithRelativeOffsetSize(relativeOffset, Length - relativeOffset);
    }

    /// <summary>
    /// Creates a new reader state with the provided offset relative to the current start offset.
    /// </summary>
    /// <param name="relativeOffset">The offset.</param>
    /// <param name="size">The size.</param>
    /// <returns>The new state.</returns>
    public readonly BinaryStreamReaderState WithRelativeOffsetSize(ulong relativeOffset, uint size)
    {
        return new BinaryStreamReaderState(
            dataSource: DataSource,
            startOffset: StartOffset + relativeOffset,
            startRva: (uint) (StartRva + relativeOffset),
            length: size,
            offset: StartOffset + relativeOffset
        );
    }

    /// <inheritdoc />
    public bool Equals(BinaryStreamReaderState other)
    {
        return DataSource.Equals(other.DataSource)
            && StartOffset == other.StartOffset
            && StartRva == other.StartRva
            && Length == other.Length
            && RelativeOffset == other.RelativeOffset;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is BinaryStreamReaderState other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = DataSource.GetHashCode();
            hashCode = (hashCode * 397) ^ StartOffset.GetHashCode();
            hashCode = (hashCode * 397) ^ (int) StartRva;
            hashCode = (hashCode * 397) ^ (int) Length;
            hashCode = (hashCode * 397) ^ RelativeOffset.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Determines whether two reader states are considered equal.
    /// </summary>
    /// <param name="a">The first state.</param>
    /// <param name="b">The second state.</param>
    /// <returns><c>true</c> if the states are equal, <c>false</c> otherwise.</returns>
    public static bool operator ==(BinaryStreamReaderState a,  BinaryStreamReaderState b) =>  a.Equals(b);

    /// <summary>
    /// Determines whether two reader states are not considered equal.
    /// </summary>
    /// <param name="a">The first state.</param>
    /// <param name="b">The second state.</param>
    /// <returns><c>true</c> if the states are not equal, <c>false</c> otherwise.</returns>
    public static bool operator !=(BinaryStreamReaderState a, BinaryStreamReaderState b) => !(a == b);
}
