using System;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Tpi;

/// <summary>
/// Represents the Type Information (TPI) stream in a PDB file.
/// </summary>
public abstract class TpiStream : SegmentBase
{
    /// <summary>
    /// Gets the default fixed MSF stream index for the TPI stream.
    /// </summary>
    public const int StreamIndex = 2;

    internal const uint TpiStreamHeaderSize =
            sizeof(TpiStreamVersion) // Version
            + sizeof(uint) // HeaderSize
            + sizeof(uint) // TypeIndexBegin
            + sizeof(uint) // TypeIndexEnd
            + sizeof(uint) // TypeRecordBytes
            + sizeof(ushort) // HashStreamIndex
            + sizeof(ushort) // HashAuxStreamIndex
            + sizeof(uint) // HashKeySize
            + sizeof(uint) // NumHashBuckets
            + sizeof(uint) // HashValueBufferOffset
            + sizeof(uint) // HashValueBufferLength
            + sizeof(uint) // IndexOffsetBufferOffset
            + sizeof(uint) // IndexOffsetBufferLength
            + sizeof(uint) // HashAdjBufferOffset
            + sizeof(uint) // HashAdjBufferLength
        ;

    /// <summary>
    /// Gets or sets the version of the file format that the TPI stream is using.
    /// </summary>
    public TpiStreamVersion Version
    {
        get;
        set;
    } = TpiStreamVersion.V80;

    /// <summary>
    /// Gets or sets the index of the first type record in the stream.
    /// </summary>
    public uint TypeIndexBegin
    {
        get;
        set;
    } = 0x1000;

    /// <summary>
    /// Gets or sets the index of the last type record in the stream.
    /// </summary>
    public uint TypeIndexEnd
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the amount of bytes the full type record data requires.
    /// </summary>
    public uint TypeRecordsByteCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the MSF stream index of the hash table for every record type in the stream (if available).
    /// </summary>
    /// <remarks>
    /// When this value is set to <c>-1</c> (<c>0xFFFF</c>), then there is no hash stream stored in the PDB file.
    /// </remarks>
    public ushort HashStreamIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the MSF stream index of the auxiliary hash table for every record type in the stream.
    /// </summary>
    /// <remarks>
    /// When this value is set to <c>-1</c> (<c>0xFFFF</c>), then there is no hash stream stored in the PDB file.
    /// The exact purpose of this stream is unknown, and usually this stream is not present.
    /// </remarks>
    public ushort HashAuxStreamIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of bytes that a single hash value in the type hash stream consists of.
    /// </summary>
    public uint HashKeySize
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of buckets used in the type record hash table.
    /// </summary>
    public uint HashBucketCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset within the TPI hash stream pointing to the start of the list of hash values.
    /// </summary>
    public uint HashValueBufferOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of bytes within the TPI hash stream that the list of hash values consists of.
    /// </summary>
    public uint HashValueBufferLength
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset within the TPI hash stream pointing to the start of the list of type record indices.
    /// </summary>
    public uint IndexOffsetBufferOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of bytes within the TPI hash stream that the list of type record indices consists of.
    /// </summary>
    public uint IndexOffsetBufferLength
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset within the TPI hash stream pointing to the start of the list of hash-index pairs.
    /// </summary>
    public uint HashAdjBufferOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of bytes within the TPI hash stream that the list of hash-index pairs consists of.
    /// </summary>
    public uint HashAdjBufferLength
    {
        get;
        set;
    }

    /// <summary>
    /// Reads a TPI stream from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <returns>The TPI stream.</returns>
    public static TpiStream FromReader(BinaryStreamReader reader) => new SerializedTpiStream(reader);

    /// <summary>
    /// Attempts to get a reader object that starts at the beginning of a leaf record for the provided type index.
    /// </summary>
    /// <param name="typeIndex">The type index to get the reader for.</param>
    /// <param name="reader">The obtained reader object.</param>
    /// <returns>
    /// <c>true</c> if the provided type index was valid and a reader object was constructed successfully,
    /// <c>false</c> otherwise.
    /// </returns>
    public abstract bool TryGetLeafRecordReader(uint typeIndex, out BinaryStreamReader reader);

    /// <summary>
    /// Gets a reader object that starts at the beginning of a leaf record for the provided type index.
    /// </summary>
    /// <param name="typeIndex">The type index to get the reader for.</param>
    /// <returns>The obtained reader object.</returns>
    /// <exception cref="ArgumentException">Occurs when the provided type index was invalid.</exception>
    public BinaryStreamReader GetLeafRecordReader(uint typeIndex)
    {
        if (!TryGetLeafRecordReader(typeIndex, out var reader))
            throw new ArgumentException("Invalid type index.");

        return reader;
    }

    /// <inheritdoc />
    public override uint GetPhysicalSize() => TpiStreamHeaderSize + TypeRecordsByteCount;

    /// <inheritdoc />
    public override void Write(IBinaryStreamWriter writer)
    {
        WriteHeader(writer);
        WriteTypeRecords(writer);
    }

    private void WriteHeader(IBinaryStreamWriter writer)
    {
        writer.WriteUInt32((uint) Version);
        writer.WriteUInt32(TpiStreamHeaderSize);
        writer.WriteUInt32(TypeIndexBegin);
        writer.WriteUInt32(TypeIndexEnd);
        writer.WriteUInt32(TypeRecordsByteCount);
        writer.WriteUInt16(HashStreamIndex);
        writer.WriteUInt16(HashAuxStreamIndex);
        writer.WriteUInt32(HashKeySize);
        writer.WriteUInt32(HashBucketCount);
        writer.WriteUInt32(HashValueBufferOffset);
        writer.WriteUInt32(HashValueBufferLength);
        writer.WriteUInt32(IndexOffsetBufferOffset);
        writer.WriteUInt32(IndexOffsetBufferLength);
        writer.WriteUInt32(HashAdjBufferOffset);
        writer.WriteUInt32(HashAdjBufferLength);
    }

    /// <summary>
    /// Writes all type records stored in the TPI stream to the provided output stream.
    /// </summary>
    /// <param name="writer">The output stream.</param>
    protected abstract void WriteTypeRecords(IBinaryStreamWriter writer);
}
