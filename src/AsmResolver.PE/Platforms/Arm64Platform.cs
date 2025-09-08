using System;
using AsmResolver.IO;
using AsmResolver.PE.File;
using AsmResolver.PE.Relocations;
using AsmResolver.Shims;

namespace AsmResolver.PE.Platforms;

/// <summary>
/// Provides information and services for the Aarch64/ARM64 target platform.
/// </summary>
public class Arm64Platform : Platform
{
    /// <summary>
    /// Gets the singleton instance for the ARM64 platform.
    /// </summary>
    public static Arm64Platform Instance
    {
        get;
    } = new();

    /// <inheritdoc />
    public override MachineType TargetMachine => MachineType.Arm64;

    /// <inheritdoc />
    public override bool IsClrBootstrapperRequired => false;

    /// <inheritdoc />
    public override bool Is32Bit => false;

    /// <inheritdoc />
    public override uint ThunkStubAlignment => 4;

    /// <inheritdoc />
    public override RelocatableSegment CreateThunkStub(ISymbol entryPoint)
    {
        // We use a custom Arm64 thunk stub segment class as AddressFixupPatch does not support ARM64 encoding yet.
        return new RelocatableSegment(
            new Arm64ThunkStub(entryPoint),
            ArrayShim.Empty<BaseRelocation>()
        );
    }

    /// <inheritdoc />
    public override bool TryExtractThunkAddress(PEImage image, BinaryStreamReader reader, out uint rva)
    {
        rva = 0;
        ulong currentPageVa = (image.ImageBase + reader.Rva) & ~0xFFFul;

        // A thunk stub in ARM64 is 3 instructions.
        if (!reader.CanRead(3 * sizeof(uint)))
            return false;

        // adrp x16, [immhi:immlo:zeros(12)]
        // Reference: https://www.scs.stanford.edu/~zyedidia/arm64/adrp.html
        uint adrp = reader.ReadUInt32();
        if ((adrp & 0b1_00_11111_0000000000000000000_11111) != 0b1_00_10000_0000000000000000000_10000)
            return false;

        uint pageAddressHi = (adrp & 0b0_00_00000_1111111111111111111_00000) >> 5;
        uint pageAddressLo = (adrp & 0b0_11_00000_0000000000000000000_00000) >> 29;
        ulong pageAddress = currentPageVa + ((pageAddressHi << 2 | pageAddressLo) << 12);

        // ldr  x16, [x16, #(&symbol & 0xFFF)]
        // Reference: https://www.scs.stanford.edu/~zyedidia/arm64/ldr_imm_gen.html
        uint ldr = reader.ReadUInt32();
        if ((ldr & 0b11_111_1_11_11_000000000000_11111_11111) != 0b11_111_0_01_01_000000000000_10000_10000)
            return false;

        ulong pageOffset = ((ldr & 0b00_000_0_00_00_111111111111_00000_00000) >> 10) << 3;

        // br x16
        // Reference: https://www.scs.stanford.edu/~zyedidia/arm64/br.html
        uint br = reader.ReadUInt32();
        if (br != 0b1101011_0_0_00_11111_0000_0_0_10000_00000)
            return false;

        ulong va = pageAddress | pageOffset;
        rva = (uint) (va - image.ImageBase);
        return true;
    }

    /// <inheritdoc />
    public override AddressTableInitializerStub CreateAddressTableInitializer(ISymbol virtualProtect)
    {
        // TODO: add support for initializer stubs.
        throw new NotSupportedException($"Address table initializer stub generation is not supported for {TargetMachine} platforms.");
    }

    private sealed class Arm64ThunkStub(ISymbol symbol) : SegmentBase
    {
        private readonly ISymbol _symbol = symbol;
        private ulong _imageBase;

        public override void UpdateOffsets(in RelocationParameters parameters)
        {
            base.UpdateOffsets(in parameters);
            _imageBase = parameters.ImageBase;
        }

        public override uint GetPhysicalSize() => 3 * sizeof(uint);

        public override void Write(BinaryStreamWriter writer)
        {
            ulong currentPageVa = (_imageBase + Rva) & ~0xFFFul;
            ulong symbolVa = _imageBase + (_symbol.GetReference()?.Rva ?? 0);

            uint shiftedPageAddress = (uint) ((symbolVa - currentPageVa) >> 12);
            uint pageAddressHi = shiftedPageAddress >> 2;
            uint pageAddressLo = shiftedPageAddress & 0b11;

            // Reference: https://www.scs.stanford.edu/~zyedidia/arm64/adrp.html
            writer.WriteUInt32(
                0b1_00_10000_0000000000000000000_10000  // adrp x16, [immhi:immlo:zeros(12)]
                | (pageAddressHi << 5)                  // + immhi
                | (pageAddressLo << 29)                 // + immlo
            );

            // ldr  x16, [x16, #(&symbol & 0xFFF)]
            // Reference: https://www.scs.stanford.edu/~zyedidia/arm64/ldr_imm_gen.html
            uint pageOffset = (uint) (symbolVa & 0b1111_1111_1111);
            writer.WriteUInt32(
                0b11_111_0_01_01_000000000000_10000_10000   // ldr  x16, [x16, imm12]
                | (pageOffset >> 3) << 10                   // + imm12
            );

            // br x16
            // Reference: https://www.scs.stanford.edu/~zyedidia/arm64/br.html
            writer.WriteUInt32(0b1101011_0_0_00_11111_0000_0_0_10000_00000);
        }
    }
}
