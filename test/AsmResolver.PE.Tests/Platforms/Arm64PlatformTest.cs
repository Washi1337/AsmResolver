using AsmResolver.PE.Platforms;
using Xunit;

namespace AsmResolver.PE.Tests.Platforms;

public class Arm64PlatformTest
{
    [Fact]
    public void CreateThunkStub()
    {
        const ulong baseAddress = 0x140000000;
        const uint fileOffset = 0x3084;
        const uint rva = 0x3c84;
        const uint corExeMainRva = 0x4150;

        var corExeMain = new Symbol(new VirtualAddress(corExeMainRva));

        var stub = Arm64Platform.Instance.CreateThunkStub(corExeMain).Segment;
        stub.UpdateOffsets(new RelocationParameters(baseAddress, fileOffset, rva, false));
        byte[] code = stub.WriteIntoArray();

        Assert.Equal(
            [
                /* 140003c84: */ 0x10, 0x00, 0x00, 0xb0,    // adrp  x16,0x140004000
                /* 140003c88: */ 0x10, 0xaa, 0x40, 0xf9,    // ldr   x16,[x16, #0x150]=>->MSCOREE.DLL::_CorExeMain
                /* 140003c8c: */ 0x00, 0x02, 0x1f, 0xd6,    // br    x16=>MSCOREE.DLL::_CorExeMain
            ],
            code
        );
    }

    [Fact]
    public void ExtractThunkRva()
    {
        var image = PEImage.FromBytes(Properties.Resources.MixedModeHelloWorld_Arm64);
        var reader = image.PEFile!.CreateReaderAtRva(image.PEFile.OptionalHeader.AddressOfEntryPoint);

        Assert.True(Arm64Platform.Instance.TryExtractThunkAddress(image, reader, out uint rva));
        Assert.Equal(0x4150u, rva);
    }
}
