using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Tpi;

public class SerializedTpiStream : TpiStream
{
    private readonly BinaryStreamReader _recordsReader;
    private List<(uint Offset, uint Length)>? _recordOffsets;

    public SerializedTpiStream(BinaryStreamReader reader)
    {
        Version = (TpiStreamVersion) reader.ReadUInt32();
        if (Version != TpiStreamVersion.V80)
            throw new NotSupportedException($"Unsupported TPI file format version {Version}.");

        uint headerSize = reader.ReadUInt32();
        if (headerSize != TpiStreamHeaderSize)
            throw new NotSupportedException("Invalid TPI header size.");

        TypeIndexBegin = reader.ReadUInt32();
        TypeIndexEnd = reader.ReadUInt32();
        TypeRecordsByteCount = reader.ReadUInt32();
        HashStreamIndex = reader.ReadUInt16();
        HashAuxStreamIndex = reader.ReadUInt16();
        HashKeySize = reader.ReadUInt32();
        HashBucketCount = reader.ReadUInt32();
        HashValueBufferOffset = reader.ReadUInt32();
        HashValueBufferLength = reader.ReadUInt32();
        IndexOffsetBufferOffset = reader.ReadUInt32();
        IndexOffsetBufferLength = reader.ReadUInt32();
        HashAdjBufferOffset = reader.ReadUInt32();
        HashAdjBufferLength = reader.ReadUInt32();

        _recordsReader = reader.ForkRelative(reader.RelativeOffset, TypeRecordsByteCount);
    }

    [MemberNotNull(nameof(_recordOffsets))]
    private void EnsureRecordOffsetMappingInitialized()
    {
        if (_recordOffsets is null)
            Interlocked.CompareExchange(ref _recordOffsets, GetRecordOffsets(), null);
    }

    private List<(uint Offset, uint Length)> GetRecordOffsets()
    {
        int count = (int) (TypeIndexEnd - TypeIndexBegin);
        var result = new List<(uint Offset, uint Length)>(count);

        var reader = _recordsReader.Fork();
        while (reader.CanRead(sizeof(ushort) * 2))
        {
            uint offset = reader.RelativeOffset;
            ushort length = reader.ReadUInt16();
            result.Add((offset, length));
            reader.Offset += length;
        }

        return result;
    }

    /// <inheritdoc />
    public override bool TryGetTypeRecordReader(uint typeIndex, out BinaryStreamReader reader)
    {
        EnsureRecordOffsetMappingInitialized();

        typeIndex -= TypeIndexBegin;
        if (typeIndex >= _recordOffsets.Count)
        {
            reader = default;
            return false;
        }

        (uint offset, uint length) = _recordOffsets[(int) typeIndex];
        reader = _recordsReader.ForkRelative(offset, length + sizeof(ushort));
        return true;
    }

    /// <inheritdoc />
    protected override void WriteTypeRecords(IBinaryStreamWriter writer)
    {
        _recordsReader.Fork().WriteToOutput(writer);
    }
}
