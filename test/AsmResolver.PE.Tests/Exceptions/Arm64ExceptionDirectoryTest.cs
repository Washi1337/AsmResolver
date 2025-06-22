using System.IO;
using AsmResolver.IO;
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

        Assert.Equal(0x1000u, exceptions.Functions[0].Begin.Rva);
        Assert.Equal(0x1018u, exceptions.Functions[0].End.Rva);
        Assert.Equal(0x1058u, exceptions.Functions[3].Begin.Rva);
        Assert.Equal(0x1080u, exceptions.Functions[3].End.Rva);
        Assert.Equal(0x1BD8u, exceptions.Functions[^1].Begin.Rva);
        Assert.Equal(0x1C08u, exceptions.Functions[^1].End.Rva);
    }

    [Fact]
    public void ReadPackedUnwindInfo()
    {
        var image = PEImage.FromBytes(Properties.Resources.NativeHelloWorldC_Arm64, TestReaderParameters);
        var exceptions = Assert.IsAssignableFrom<ExceptionDirectory<Arm64RuntimeFunction>>(image.Exceptions);
        Assert.IsAssignableFrom<Arm64PackedUnwindInfo>(exceptions.Functions[3].UnwindInfo);
    }

    [Fact]
    public void ReadUnpackedUnwindInfo()
    {
        var image = PEImage.FromBytes(Properties.Resources.NativeHelloWorldC_Arm64, TestReaderParameters);
        var exceptions = Assert.IsAssignableFrom<ExceptionDirectory<Arm64RuntimeFunction>>(image.Exceptions);
        Assert.IsAssignableFrom<Arm64UnpackedUnwindInfo>(exceptions.Functions[0].UnwindInfo);
    }

    [Fact]
    public void RoundtripPackedUnwindInfo()
    {
        // Read original packed info
        var image = PEImage.FromBytes(Properties.Resources.NativeHelloWorldC_Arm64, TestReaderParameters);
        var exceptions = Assert.IsAssignableFrom<ExceptionDirectory<Arm64RuntimeFunction>>(image.Exceptions);
        var function = exceptions.Functions[3];
        var unwindInfo = Assert.IsAssignableFrom<Arm64PackedUnwindInfo>(function.UnwindInfo);

        // Write
        using var stream = new MemoryStream();
        function.Write(new BinaryStreamWriter(stream));

        // Re-read
        var context = new PEReaderContext(image.PEFile!, TestReaderParameters);
        var reader = new BinaryStreamReader(stream.ToArray());
        var newFunction = Arm64RuntimeFunction.FromReader(context, ref reader);
        var newUnwindInfo = Assert.IsAssignableFrom<Arm64PackedUnwindInfo>(newFunction.UnwindInfo);

        // Verify equivalence
        Assert.Equal(unwindInfo.FunctionLength, newUnwindInfo.FunctionLength);
        Assert.Equal(unwindInfo.FPRegisterCount, newUnwindInfo.FPRegisterCount);
        Assert.Equal(unwindInfo.IntegerRegisterCount, newUnwindInfo.IntegerRegisterCount);
        Assert.Equal(unwindInfo.HomesRegisters, newUnwindInfo.HomesRegisters);
        Assert.Equal(unwindInfo.CR, newUnwindInfo.CR);
        Assert.Equal(unwindInfo.FrameSize, newUnwindInfo.FrameSize);
    }

    [Fact]
    public void RoundtripUnpackedUnwindInfo()
    {
        // Read original unpacked info
        var image = PEImage.FromBytes(Properties.Resources.NativeHelloWorldC_Arm64, TestReaderParameters);
        var exceptions = Assert.IsAssignableFrom<ExceptionDirectory<Arm64RuntimeFunction>>(image.Exceptions);
        var function = exceptions.Functions[7];
        var unwindInfo = Assert.IsAssignableFrom<Arm64UnpackedUnwindInfo>(function.UnwindInfo);

        // Write
        using var stream = new MemoryStream();
        unwindInfo.Write(new BinaryStreamWriter(stream));

        // Reread
        var context = new PEReaderContext(image.PEFile!, TestReaderParameters);
        var reader = new BinaryStreamReader(stream.ToArray());
        var newUnwindInfo = Arm64UnpackedUnwindInfo.FromReader(context, ref reader);

        // Verify equivalence
        Assert.Equal(unwindInfo.FunctionLength, newUnwindInfo.FunctionLength);
        Assert.Equal(unwindInfo.Version, newUnwindInfo.Version);
        Assert.Equal(unwindInfo.EpilogScopes, newUnwindInfo.EpilogScopes);
        Assert.Equal(unwindInfo.UnwindCodes, newUnwindInfo.UnwindCodes);
        Assert.Equal(unwindInfo.ExceptionHandlerData.Rva, newUnwindInfo.ExceptionHandlerData.Rva);
    }
}
