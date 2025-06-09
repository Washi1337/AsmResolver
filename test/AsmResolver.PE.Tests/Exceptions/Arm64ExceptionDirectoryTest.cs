using AsmResolver.PE.Exceptions;
using Xunit;

namespace AsmResolver.PE.Tests.Exceptions;

public class Arm64ExceptionDirectoryTest
{
    [Fact]
    public void ReadSEHTable()
    {
        var image = PEImage.FromBytes(Properties.Resources.NativeHelloWorldC_Arm64, TestReaderParameters);
        var exceptions = Assert.IsAssignableFrom<ExceptionDirectory<Arm64RuntimeFunction>>(image.Exceptions);

        Assert.Equal(0x1000u, exceptions.Entries[0].Begin.Rva);
        Assert.Equal(0x1018u, exceptions.Entries[0].End.Rva);
        Assert.Equal(0x1058u, exceptions.Entries[3].Begin.Rva);
        Assert.Equal(0x1080u, exceptions.Entries[3].End.Rva);
        Assert.Equal(0x1BD8u, exceptions.Entries[^1].Begin.Rva);
        Assert.Equal(0x1C08u, exceptions.Entries[^1].End.Rva);
    }

    [Fact]
    public void ReadPackedUnwindInfo()
    {
        var image = PEImage.FromBytes(Properties.Resources.NativeHelloWorldC_Arm64, TestReaderParameters);
        var exceptions = Assert.IsAssignableFrom<ExceptionDirectory<Arm64RuntimeFunction>>(image.Exceptions);
        Assert.IsAssignableFrom<Arm64PackedUnwindInfo>(exceptions.Entries[3].UnwindInfo);
    }

    [Fact]
    public void ReadXDataUnwindInfo()
    {
        var image = PEImage.FromBytes(Properties.Resources.NativeHelloWorldC_Arm64, TestReaderParameters);
        var exceptions = Assert.IsAssignableFrom<ExceptionDirectory<Arm64RuntimeFunction>>(image.Exceptions);
        Assert.IsAssignableFrom<Arm64XDataUnwindInfo>(exceptions.Entries[0].UnwindInfo);
    }
}
