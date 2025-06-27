using AsmResolver.PE.Platforms;
using Xunit;

namespace AsmResolver.PE.Tests.Platforms;

public class Amd64PlatformTest
{
    [Fact]
    public void CreateThunkStub()
    {
        const ulong baseAddress = 0x00400000;
        const uint fileOffset = 0x3000;
        const uint rva = 0x237e;
        const uint corExeMainRva = 0x2000;

        var corExeMain = new Symbol(new VirtualAddress(corExeMainRva));

        var stub = Amd64Platform.Instance.CreateThunkStub(corExeMain).Segment;
        stub.UpdateOffsets(new RelocationParameters(baseAddress, fileOffset, rva, false));
        byte[] code = stub.WriteIntoArray();

        Assert.Equal(
            [
                0x48, 0xA1, 0x00, 0x20, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, // rex.w rex.b mov rax, [&symbol]
                0xFF, 0xE0                                                  // jmp [rax]
            ],
            code
        );
    }

    [Fact]
    public void ExtractThunkRvaRipRelative()
    {
        var image = PEImage.FromBytes(Properties.Resources.MixedModeHelloWorld_X64);
        var reader = image.PEFile!.CreateReaderAtRva(image.PEFile.OptionalHeader.AddressOfEntryPoint);

        Assert.True(Amd64Platform.Instance.TryExtractThunkAddress(image, reader, out uint rva));
        Assert.Equal(0x5188u, rva);
    }

    [Fact]
    public void ExtractThunkRvaAbsolute()
    {
        var image = PEImage.FromBytes(Properties.Resources.UnmanagedExports_x64);
        var reader = image.PEFile!.CreateReaderAtRva(image.PEFile.OptionalHeader.AddressOfEntryPoint);

        Assert.True(Amd64Platform.Instance.TryExtractThunkAddress(image, reader, out uint rva));
        Assert.Equal(0x2000u, rva);
    }
}
