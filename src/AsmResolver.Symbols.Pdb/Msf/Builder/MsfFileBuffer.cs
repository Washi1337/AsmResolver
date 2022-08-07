using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Msf.Builder;

/// <summary>
/// Represents a mutable buffer for building up a new MSF file.
/// </summary>
public class MsfFileBuffer : SegmentBase
{
    private readonly Dictionary<MsfStream, int[]> _blockIndices = new();
    private readonly List<FreeBlockMap> _freeBlockMaps = new(2);
    private readonly List<ISegment?> _blocks;

    /// <summary>
    /// Creates a new empty MSF file buffer.
    /// </summary>
    /// <param name="blockSize">The block size to use.</param>
    public MsfFileBuffer(uint blockSize)
    {
        SuperBlock = new MsfSuperBlock
        {
            Signature = MsfSuperBlock.BigMsfSignature,
            BlockSize = blockSize,
            FreeBlockMapIndex = 1,
            BlockCount = 3,
        };

        _blocks = new List<ISegment?>((int) blockSize);

        InsertBlock(0, SuperBlock);
        var fpm = GetOrCreateFreeBlockMap(1, out _);
        InsertBlock(2, null);

        fpm.BitField[0] = false;
        fpm.BitField[1] = false;
        fpm.BitField[2] = false;
    }

    /// <summary>
    /// Gets the super block of the MSF file that is being constructed.
    /// </summary>
    public MsfSuperBlock SuperBlock
    {
        get;
    }

    /// <summary>
    /// Determines whether a block in the MSF file buffer is available or not.
    /// </summary>
    /// <param name="blockIndex">The index of the block.</param>
    /// <returns><c>true</c> if the block is available, <c>false</c> otherwise.</returns>
    public bool BlockIsAvailable(int blockIndex)
    {
        var freeBlockMap = GetOrCreateFreeBlockMap(blockIndex, out int offset);
        if (offset < 3 && (blockIndex == 0 || offset > 0))
            return false;
        return freeBlockMap.BitField[offset];
    }

    /// <summary>
    /// Inserts a block of the provided MSF stream into the buffer.
    /// </summary>
    /// <param name="blockIndex">The MSF file index to insert the block into.</param>
    /// <param name="stream">The stream to pull a chunk from.</param>
    /// <param name="chunkIndex">The index of the chunk to store at the provided block index.</param>
    /// <exception cref="ArgumentException">
    /// Occurs when the index provided by <paramref name="blockIndex"/> is already in use.
    /// </exception>
    public void InsertBlock(int blockIndex, MsfStream stream, int chunkIndex)
    {
        var fpm = GetOrCreateFreeBlockMap(blockIndex, out int offset);
        if (!fpm.BitField[offset])
            throw new ArgumentException($"Block {blockIndex} is already in use.");

        uint blockSize = SuperBlock.BlockSize;
        var segment = new DataSourceSegment(
            stream.Contents,
            stream.Contents.BaseAddress + (ulong) (chunkIndex * blockSize),
            (uint) (chunkIndex * blockSize),
            (uint) Math.Min(stream.Contents.Length - (ulong) (chunkIndex * blockSize), blockSize));

        InsertBlock(blockIndex, segment);

        int[] indices = GetMutableBlockIndicesForStream(stream);
        indices[chunkIndex] = blockIndex;

        fpm.BitField[offset] = false;
    }

    private void InsertBlock(int blockIndex, ISegment? segment)
    {
        // Ensure enough blocks are present in the backing-buffer.
        while (_blocks.Count <= blockIndex)
            _blocks.Add(null);

        // Insert block and update super block.
        _blocks[blockIndex] = segment;
        SuperBlock.BlockCount = (uint) _blocks.Count;
    }

    private FreeBlockMap GetOrCreateFreeBlockMap(int blockIndex, out int offset)
    {
        int index = Math.DivRem(blockIndex, (int) SuperBlock.BlockSize, out offset);
        while (_freeBlockMaps.Count <= index)
        {
            var freeBlockMap = new FreeBlockMap(SuperBlock.BlockSize);
            _freeBlockMaps.Add(freeBlockMap);
            InsertBlock(index + (int) SuperBlock.FreeBlockMapIndex, freeBlockMap);
        }

        return _freeBlockMaps[index];
    }

    private int[] GetMutableBlockIndicesForStream(MsfStream stream)
    {
        if (!_blockIndices.TryGetValue(stream, out int[]? indices))
        {
            indices = new int[stream.GetRequiredBlockCount(SuperBlock.BlockSize)];
            _blockIndices.Add(stream, indices);
        }

        return indices;
    }

    /// <summary>
    /// Gets the allocated indices for the provided MSF stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>The block indices.</returns>
    public int[] GetBlockIndicesForStream(MsfStream stream) => (int[]) GetMutableBlockIndicesForStream(stream).Clone();

    /// <summary>
    /// Constructs a new MSF stream containing the stream directory.
    /// </summary>
    /// <param name="streams">The files that the directory should list.</param>
    /// <returns>The constructed stream.</returns>
    /// <remarks>
    /// This method does <b>not</b> add the stream to the buffer, nor does it update the super block.
    /// </remarks>
    public MsfStream CreateStreamDirectory(IList<MsfStream> streams)
    {
        using var contents = new MemoryStream();
        var writer = new BinaryStreamWriter(contents);

        // Stream count.
        writer.WriteInt32(streams.Count);

        // Stream sizes.
        for (int i = 0; i < streams.Count; i++)
            writer.WriteUInt32((uint) streams[i].Contents.Length);

        // Stream indices.
        for (int i = 0; i < streams.Count; i++)
        {
            int[] indices = GetMutableBlockIndicesForStream(streams[i]);
            foreach (int index in indices)
                writer.WriteInt32(index);
        }

        return new MsfStream(contents.ToArray());
    }

    /// <summary>
    /// Creates a new MSF stream containing the block indices of the stream directory.
    /// </summary>
    /// <param name="streamDirectory">The stream directory to store the indices for.</param>
    /// <returns>The constructed stream.</returns>
    /// <remarks>
    /// This method does <b>not</b> add the stream to the buffer, nor does it update the super block.
    /// </remarks>
    public MsfStream CreateStreamDirectoryMap(MsfStream streamDirectory)
    {
        using var contents = new MemoryStream();
        var writer = new BinaryStreamWriter(contents);

        int[] indices = GetMutableBlockIndicesForStream(streamDirectory);
        foreach (int index in indices)
            writer.WriteInt32(index);

        return new MsfStream(contents.ToArray());
    }

    /// <inheritdoc />
    public override uint GetPhysicalSize() => SuperBlock.BlockCount * SuperBlock.BlockSize;

    /// <inheritdoc />
    public override void Write(IBinaryStreamWriter writer)
    {
        foreach (var block in _blocks)
        {
            if (block is null)
            {
                writer.WriteZeroes((int) SuperBlock.BlockSize);
            }
            else
            {
                block.Write(writer);
                writer.Align(SuperBlock.BlockSize);
            }
        }
    }
}
