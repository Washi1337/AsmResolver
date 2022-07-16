using System;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Tpi;

public abstract class TpiStream : SegmentBase
{
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

    public TpiStreamVersion Version
    {
        get;
        set;
    } = TpiStreamVersion.V80;

    public uint TypeIndexBegin
    {
        get;
        set;
    } = 0x1000;

    public uint TypeIndexEnd
    {
        get;
        set;
    }

    public uint TypeRecordsByteCount
    {
        get;
        set;
    }

    public ushort HashStreamIndex
    {
        get;
        set;
    }

    public ushort HashAuxStreamIndex
    {
        get;
        set;
    }

    public uint HashKeySize
    {
        get;
        set;
    }

    public uint HashBucketCount
    {
        get;
        set;
    }

    public uint HashValueBufferOffset
    {
        get;
        set;
    }

    public uint HashValueBufferLength
    {
        get;
        set;
    }

    public uint IndexOffsetBufferOffset
    {
        get;
        set;
    }

    public uint IndexOffsetBufferLength
    {
        get;
        set;
    }

    public uint HashAdjBufferOffset
    {
        get;
        set;
    }

    public uint HashAdjBufferLength
    {
        get;
        set;
    }

    public static TpiStream FromReader(BinaryStreamReader reader) => new SerializedTpiStream(reader);

    public abstract bool TryGetTypeRecordReader(uint typeIndex, out BinaryStreamReader reader);

    public BinaryStreamReader GetTypeRecordReader(uint typeIndex)
    {
        if (!TryGetTypeRecordReader(typeIndex, out var reader))
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

    protected abstract void WriteTypeRecords(IBinaryStreamWriter writer);
}
