using AsmResolver.PE.Platforms;
using Xunit;

namespace AsmResolver.PE.Tests.Platforms;

public class Arm32PlatformTest
{
    [Fact]
    public void CreateThunkStub()
    {
        const ulong baseAddress = 0x00400000;
        const uint fileOffset = 0x3000;
        const uint rva = 0x268e;
        const uint corExeMainRva = 0x2000;

        var corExeMain = new Symbol(new VirtualAddress(corExeMainRva));

        var stub = Arm32Platform.Instance.CreateThunkStub(corExeMain).Segment;
        stub.UpdateOffsets(new RelocationParameters(baseAddress, fileOffset, rva, true));
        byte[] code = stub.WriteIntoArray();

        Assert.Equal(
            [
                /* 00: */ 0xDF, 0xF8, 0x00, 0xF0, // ldr pc, [pc, #0]
                /* 04: */ 0x00, 0x20, 0x40, 0x00, // <address>
            ],
            code
        );
    }
}
