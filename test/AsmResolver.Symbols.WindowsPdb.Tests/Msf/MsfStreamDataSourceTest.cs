using System;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.Symbols.WindowsPdb.Msf;
using Xunit;

namespace AsmResolver.Symbols.WindowsPdb.Tests.Msf;

public class MsfStreamDataSourceTest
{
    [Fact]
    public void EmptyStream()
    {
        var source = new MsfStreamDataSource(0, 0x200, Array.Empty<IDataSource>());

        byte[] buffer = new byte[0x1000];
        int readCount = source.ReadBytes(0, buffer, 0, buffer.Length);
        Assert.Equal(0, readCount);
        Assert.All(buffer, b => Assert.Equal(b, 0));
    }

    [Theory]
    [InlineData(0x200, 0x200)]
    [InlineData(0x200, 0x100)]
    public void StreamWithOneBlock(int blockSize, int actualSize)
    {
        byte[] block = new byte[blockSize];
        for (int i = 0; i < blockSize; i++)
            block[i] = (byte) (i & 0xFF);

        var source = new MsfStreamDataSource((ulong) actualSize, (uint) blockSize, new[] {
            block
        });

        byte[] buffer = new byte[0x1000];
        int readCount = source.ReadBytes(0, buffer, 0, buffer.Length);
        Assert.Equal(actualSize, readCount);
        Assert.Equal(block.Take(actualSize), buffer.Take(actualSize));
    }

    [Theory]
    [InlineData(0x200, 0x400)]
    [InlineData(0x200, 0x300)]
    public void StreamWithTwoBlocks(int blockSize, int actualSize)
    {
        byte[] block1 = new byte[blockSize];
        for (int i = 0; i < blockSize; i++)
            block1[i] = (byte) 'A';

        byte[] block2 = new byte[blockSize];
        for (int i = 0; i < blockSize; i++)
            block2[i] = (byte) 'B';

        var source = new MsfStreamDataSource((ulong) actualSize, (uint) blockSize, new[] {block1, block2});

        byte[] buffer = new byte[0x1000];
        int readCount = source.ReadBytes(0, buffer, 0, buffer.Length);
        Assert.Equal(actualSize, readCount);
        Assert.Equal(block1.Concat(block2).Take(actualSize), buffer.Take(actualSize));
    }

    [Theory]
    [InlineData(0x200, 0x400)]
    public void ReadInMiddleOfBlock(int blockSize, int actualSize)
    {
        byte[] block1 = new byte[blockSize];
        for (int i = 0; i < blockSize; i++)
            block1[i] = (byte) ((i*2) & 0xFF);

        byte[] block2 = new byte[blockSize];
        for (int i = 0; i < blockSize; i++)
            block2[i] = (byte) ((i * 2 + 1) & 0xFF);

        var source = new MsfStreamDataSource((ulong) actualSize, (uint) blockSize, new[] {block1, block2});

        byte[] buffer = new byte[blockSize];
        int readCount = source.ReadBytes((ulong) blockSize / 4, buffer, 0, blockSize);
        Assert.Equal(blockSize, readCount);
        Assert.Equal(block1.Skip(blockSize / 4).Concat(block2).Take(blockSize), buffer);
    }
}
