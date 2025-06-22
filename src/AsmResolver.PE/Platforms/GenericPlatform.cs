using System;
using AsmResolver.IO;
using AsmResolver.PE.File;
using AsmResolver.PE.Relocations;

namespace AsmResolver.PE.Platforms;

/// <summary>
/// Represents the generic fallback target platform.
/// </summary>
/// <remarks>
/// <para>
/// This class implements the bare minimum for any platform that is not supported through a dedicated
/// <see cref="Platform"/> class. It is therefore also not able to generate native code (such as thunk stubs or
/// IAT initializers), and will throw <see cref="NotSupportedException"/> when instructed to do so.
/// </para>
/// <para>
/// Note that for .NET binaries, the generic platform is not the same as AnyCPU.
/// AnyCPU binaries are compiled as if they were i386 binaries and thus use the <see cref="I386Platform"/> instead.
/// </para>
/// </remarks>
public sealed class GenericPlatform : Platform
{
    internal GenericPlatform(MachineType targetMachine)
    {
        TargetMachine = targetMachine;
    }

    /// <inheritdoc />
    public override MachineType TargetMachine
    {
        get;
    }

    /// <inheritdoc />
    public override bool Is32Bit => true;

    /// <inheritdoc />
    public override bool IsClrBootstrapperRequired => false;

    /// <inheritdoc />
    public override uint ThunkStubAlignment => 1;

    /// <inheritdoc />
    public override RelocatableSegment CreateThunkStub(ISymbol entryPoint)
    {
        throw new NotSupportedException($"Thunk stub generation is not supported for {TargetMachine} platforms.");
    }

    /// <inheritdoc />
    public override bool TryExtractThunkAddress(PEImage image, BinaryStreamReader reader, out uint rva)
    {
        rva = 0;
        return false;
    }

    /// <inheritdoc />
    public override AddressTableInitializerStub CreateAddressTableInitializer(ISymbol virtualProtect)
    {
        throw new NotSupportedException($"Address table initializer stub generation is not supported for {TargetMachine} platforms.");
    }
}
