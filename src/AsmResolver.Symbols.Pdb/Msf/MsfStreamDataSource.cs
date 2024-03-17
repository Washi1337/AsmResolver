using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Msf;

/// <summary>
/// Implements a data source for a single MSF stream that pulls data from multiple (fragmented) blocks.
/// </summary>
public class MsfStreamDataSource : IDataSource
{
    private readonly IDataSource[] _blocks;
    private readonly long _blockSize;

    /// <summary>
    /// Creates a new MSF stream data source.
    /// </summary>
    /// <param name="length">The length of the stream.</param>
    /// <param name="blockSize">The size of an individual block.</param>
    /// <param name="blocks">The blocks</param>
    /// <exception cref="ArgumentException">
    /// Occurs when the total size of the provided blocks is smaller than
    /// <paramref name="length"/> * <paramref name="blockSize"/>.
    /// </exception>
    public MsfStreamDataSource(ulong length, uint blockSize, IEnumerable<byte[]> blocks)
        : this(length, blockSize, blocks.Select(x => (IDataSource) new ByteArrayDataSource(x)))
    {
    }

    /// <summary>
    /// Creates a new MSF stream data source.
    /// </summary>
    /// <param name="length">The length of the stream.</param>
    /// <param name="blockSize">The size of an individual block.</param>
    /// <param name="blocks">The blocks</param>
    /// <exception cref="ArgumentException">
    /// Occurs when the total size of the provided blocks is smaller than
    /// <paramref name="length"/> * <paramref name="blockSize"/>.
    /// </exception>
    public MsfStreamDataSource(ulong length, uint blockSize, IEnumerable<IDataSource> blocks)
    {
        Length = length;
        _blocks = blocks.ToArray();
        _blockSize = blockSize;

        if (length > (ulong) (_blocks.Length * blockSize))
            throw new ArgumentException("Provided length is larger than the provided blocks combined.");
    }

    /// <inheritdoc />
    public ulong BaseAddress => 0;

    /// <inheritdoc />
    public ulong Length
    {
        get;
    }

    /// <inheritdoc />
    public byte this[ulong address]
    {
        get
        {
            if (!IsValidAddress(address))
                throw new IndexOutOfRangeException();

            var block = GetBlockAndOffset(address, out ulong offset);
            return block[block.BaseAddress + offset];
        }
    }

    /// <inheritdoc />
    public bool IsValidAddress(ulong address) => address < Length;

    /// <inheritdoc />
    public int ReadBytes(ulong address, byte[] buffer, int index, int count)
    {
        int totalReadCount = 0;
        int remainingBytes = Math.Min(count, (int) (Length - (address - BaseAddress)));

        while (remainingBytes > 0)
        {
            // Obtain current block and offset within block.
            var block = GetBlockAndOffset(address, out ulong offset);

            // Read available bytes.
            int readCount = Math.Min(remainingBytes, (int) _blockSize);
            int actualReadCount = block.ReadBytes(block.BaseAddress + offset, buffer, index, readCount);

            // Move to the next block.
            totalReadCount += actualReadCount;
            address += (ulong) actualReadCount;
            index += actualReadCount;
            remainingBytes -= actualReadCount;
        }

        return totalReadCount;
    }

    private IDataSource GetBlockAndOffset(ulong address, out ulong offset)
    {
        var block = _blocks[Math.DivRem((long) address, _blockSize, out long x)];
        offset = (ulong) x;
        return block;
    }
}
