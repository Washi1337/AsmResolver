using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.IO;

namespace AsmResolver.Symbols.WindowsPdb.Msf;

/// <summary>
/// Provides an implementation for an MSF file version, read from an input file.
/// </summary>
/// <remarks>
/// Currently, this model only supports version 7.0 of the file format.
/// </remarks>
public class SerializedMsfFile : MsfFile
{
    private readonly BinaryStreamReader _reader;
    private readonly uint _originalBlockSize;

    private readonly IDataSource?[] _blocks;
    private readonly uint _directoryByteCount;
    private readonly int _blockMapIndex;

    /// <summary>
    /// Interprets an input stream as an MSF file version 7.0.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <exception cref="BadImageFormatException">Occurs when the MSF file is malformed.</exception>
    public SerializedMsfFile(BinaryStreamReader reader)
    {
        // Check MSF header.
        byte[] signature = new byte[BigMsfSignature.Length];
        int count = reader.ReadBytes(signature, 0, signature.Length);
        if (count != BigMsfSignature.Length || !ByteArrayEqualityComparer.Instance.Equals(signature, BigMsfSignature))
            throw new BadImageFormatException("File does not start with a valid or supported MSF file signature.");

        // BlockSize property also validates, so no need to do it again.
        BlockSize = _originalBlockSize = reader.ReadUInt32();

        // We don't really use the free block map as we are not fully implementing the NTFS-esque file system, but we
        // validate its contents regardless as a sanity check.
        int freeBlockMapIndex = reader.ReadInt32();
        if (freeBlockMapIndex is not (1 or 2))
            throw new BadImageFormatException($"Free block map index must be 1 or 2, but was {freeBlockMapIndex}.");

        int blockCount = reader.ReadInt32();
        _blocks = new IDataSource?[blockCount];

        _directoryByteCount = reader.ReadUInt32();
        reader.Offset += sizeof(uint);
        _blockMapIndex = reader.ReadInt32();

        _reader = reader;
    }

    private IDataSource GetBlock(int index)
    {
        if (_blocks[index] is null)
        {
            // We lazily initialize all blocks by slicing the original data source of the reader.
            var block = new DataSourceSlice(
                _reader.DataSource,
                _reader.DataSource.BaseAddress + (ulong) (index * _originalBlockSize),
                _originalBlockSize);

            Interlocked.CompareExchange(ref _blocks[index], block, null);
        }

        return _blocks[index]!;
    }

    /// <inheritdoc />
    protected override IList<MsfStream> GetStreams()
    {
        // Get the block indices of the Stream Directory stream.
        var indicesBlock = GetBlock(_blockMapIndex);
        var indicesReader = new BinaryStreamReader(indicesBlock, indicesBlock.BaseAddress, 0,
            GetBlockCount(_directoryByteCount) * sizeof(uint));

        // Access the Stream Directory stream.
        var directoryStream = CreateStreamFromIndicesReader(ref indicesReader, _directoryByteCount);
        var directoryReader = directoryStream.CreateReader();

        // Stream Directory format is as follows:
        // - stream count: uint32
        // - stream sizes: uint32[stream count]
        // - stream indices: uint32[stream count][]

        int streamCount = directoryReader.ReadInt32();

        // Read sizes.
        uint[] streamSizes = new uint[streamCount];
        for (int i = 0; i < streamCount; i++)
            streamSizes[i] = directoryReader.ReadUInt32();

        // Construct streams.
        var result = new List<MsfStream>(streamCount);
        for (int i = 0; i < streamCount; i++)
        {
            // A size of 0xFFFFFFFF indicates the stream does not exist.
            if (streamSizes[i] == uint.MaxValue)
                continue;

            result.Add(CreateStreamFromIndicesReader(ref directoryReader, streamSizes[i]));
        }

        return result;
    }

    private MsfStream CreateStreamFromIndicesReader(ref BinaryStreamReader indicesReader, uint streamSize)
    {
        // Read all indices.
        int[] indices = new int[GetBlockCount(streamSize)];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = indicesReader.ReadInt32();

        // Transform indices to blocks.
        var blocks = new IDataSource[indices.Length];
        for (int i = 0; i < blocks.Length; i++)
            blocks[i] = GetBlock(indices[i]);

        // Construct stream.
        var dataSource = new MsfStreamDataSource(streamSize, _originalBlockSize, blocks);
        return new MsfStream(dataSource, indices);
    }

    private uint GetBlockCount(uint streamSize) => (streamSize + _originalBlockSize - 1) / _originalBlockSize;
}
