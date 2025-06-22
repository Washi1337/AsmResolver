using AsmResolver.PE.Platforms;
using Xunit;

namespace AsmResolver.PE.Tests.Platforms;

public class I386PlatformTest
{
    [Fact]
    public void CreateThunkStub()
    {
        const ulong baseAddress = 0x00400000;
        const uint fileOffset = 0x3000;
        const uint rva = 0x2f2a;
        const uint corExeMainRva = 0x30d0;

        var corExeMain = new Symbol(new VirtualAddress(corExeMainRva));

        var stub = I386Platform.Instance.CreateThunkStub(corExeMain).Segment;
        stub.UpdateOffsets(new RelocationParameters(baseAddress, fileOffset, rva, true));
        byte[] code = stub.WriteIntoArray();

        Assert.Equal(
            [
                /* 00: */ 0xFF, 0x25,             // jmp dword [&address]
                /* 02: */ 0xD0, 0x30, 0x40, 0x00, // <address>
            ],
            code
        );
    }

    [Fact]
    public void ExtractThunkRva()
    {
        var image = PEImage.FromBytes(Properties.Resources.MixedModeCallIntoNative);
        var reader = image.PEFile!.CreateReaderAtRva(image.PEFile.OptionalHeader.AddressOfEntryPoint);

        Assert.True(I386Platform.Instance.TryExtractThunkAddress(image, reader, out uint rva));
        Assert.Equal(0x30D0u, rva);
    }
}
