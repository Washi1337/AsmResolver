using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.IO;

namespace AsmResolver.Symbols.WindowsPdb.Msf;

/// <summary>
/// Provides an implementation for an MSF file that is read from an input file.
/// </summary>
/// <remarks>
/// Currently, this model only supports version 7.0 of the file format.
/// </remarks>
public class SerializedMsfFile : MsfFile
{
    private readonly BinaryStreamReader _reader;
    private readonly MsfSuperBlock _originalSuperBlock;
    private readonly IDataSource?[] _blocks;

    /// <summary>
    /// Interprets an input stream as an MSF file version 7.0.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <exception cref="BadImageFormatException">Occurs when the MSF file is malformed.</exception>
    public SerializedMsfFile(BinaryStreamReader reader)
    {
        _originalSuperBlock = MsfSuperBlock.FromReader(ref reader);

        BlockSize = _originalSuperBlock.BlockSize;
        _blocks = new IDataSource?[_originalSuperBlock.BlockCount];
        _reader = reader;
    }

    private IDataSource GetBlock(int index)
    {
        if (_blocks[index] is null)
        {
            // We lazily initialize all blocks by slicing the original data source of the reader.
            var block = new DataSourceSlice(
                _reader.DataSource,
                _reader.DataSource.BaseAddress + (ulong) (index * _originalSuperBlock.BlockSize),
                _originalSuperBlock.BlockSize);

            Interlocked.CompareExchange(ref _blocks[index], block, null);
        }

        return _blocks[index]!;
    }

    /// <inheritdoc />
    protected override IList<MsfStream> GetStreams()
    {
        // Get the block indices of the Stream Directory stream.
        var indicesBlock = GetBlock((int) _originalSuperBlock.DirectoryMapIndex);
        var indicesReader = new BinaryStreamReader(indicesBlock, indicesBlock.BaseAddress, 0,
            GetBlockCount(_originalSuperBlock.DirectoryByteCount) * sizeof(uint));

        // Access the Stream Directory stream.
        var directoryStream = CreateStreamFromIndicesReader(ref indicesReader, _originalSuperBlock.DirectoryByteCount);
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
        var result = new OwnedCollection<MsfFile, MsfStream>(this, streamCount);
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
        var dataSource = new MsfStreamDataSource(streamSize, _originalSuperBlock.BlockSize, blocks);
        return new MsfStream(dataSource, indices);
    }

    private uint GetBlockCount(uint streamSize)
    {
        return (streamSize + _originalSuperBlock.BlockSize - 1) / _originalSuperBlock.BlockSize;
    }
}
