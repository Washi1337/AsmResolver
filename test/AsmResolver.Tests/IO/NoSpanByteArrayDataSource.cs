using System;
using AsmResolver.IO;

namespace AsmResolver.Tests.IO;

public sealed class NoSpanByteArrayDataSource : IDataSource
{
    private readonly byte[] _data;

    public NoSpanByteArrayDataSource(byte[] data)
        : this(data, 0)
    {
    }

    public NoSpanByteArrayDataSource(byte[] data, ulong baseAddress)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        BaseAddress = baseAddress;
    }

    /// <inheritdoc />
    public ulong BaseAddress
    {
        get;
    }

    /// <inheritdoc />
    public byte this[ulong address] => _data[address - BaseAddress];

    /// <inheritdoc />
    public ulong Length => (ulong) _data.Length;

    /// <inheritdoc />
    public bool IsValidAddress(ulong address) => address - BaseAddress < (ulong) _data.Length;

    /// <inheritdoc />
    public int ReadBytes(ulong address, byte[] buffer, int index, int count)
    {
        int relativeIndex = (int) (address - BaseAddress);
        int actualLength = Math.Min(count, _data.Length - relativeIndex);
        Buffer.BlockCopy(_data, relativeIndex, buffer, index, actualLength);
        return actualLength;
    }

    /// <inheritdoc />
    public void ReadBytes(ulong address, Span<byte> buffer)
    {
        _data.AsSpan((int) (address - BaseAddress), buffer.Length).CopyTo(buffer);
    }

    /// <inheritdoc />
    public bool TryGetSpan(ulong address, int length, out ReadOnlySpan<byte> span)
    {
        span = default;
        return false;
    }
}
