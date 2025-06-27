using System;
using AsmResolver.IO;
using AsmResolver.PE.Code;
using AsmResolver.PE.File;
using AsmResolver.PE.Relocations;

namespace AsmResolver.PE.Platforms;

/// <summary>
/// Provides information and services for the ARM32 target platform.
/// </summary>
public class Arm32Platform : Platform
{
    /// <summary>
    /// Gets the singleton instance for the ARM32 platform.
    /// </summary>
    public static Arm32Platform Instance
    {
        get;
    } = new();

    /// <inheritdoc />
    public override MachineType TargetMachine => MachineType.ArmNt;

    /// <inheritdoc />
    public override bool IsClrBootstrapperRequired => false;

    /// <inheritdoc />
    public override bool Is32Bit => true;

    /// <inheritdoc />
    public override uint ThunkStubAlignment => sizeof(uint);

    /// <inheritdoc />
    public override RelocatableSegment CreateThunkStub(ISymbol entryPoint)
    {
        var segment = new DataSegment([
                /* 00: */ 0xDF, 0xF8, 0x00, 0xF0, // ldr pc, [pc, #0]
                /* 04: */ 0x00, 0x00, 0x00, 0x00, // <address>
            ])
            .AsPatchedSegment()
            .Patch(4, AddressFixupType.Absolute32BitAddress, entryPoint);

        return new RelocatableSegment(segment, [
            new BaseRelocation(RelocationType.HighLow, segment.ToReference(4))
        ]);
    }

    /// <inheritdoc />
    public override bool TryExtractThunkAddress(PEImage image, BinaryStreamReader reader, out uint rva)
    {
        // 00: ldr pc, [pc, #0]
        // 04: <address>

        if (reader.ReadUInt32() != 0xF000F8DF)
        {
            rva = 0;
            return false;
        }

        rva = (uint) (reader.ReadUInt32() - image.ImageBase);
        return true;
    }

    /// <inheritdoc />
    public override AddressTableInitializerStub CreateAddressTableInitializer(ISymbol virtualProtect)
    {
        throw new NotSupportedException($"Address table initializer stub generation is not supported for {TargetMachine} platforms.");
    }
}
