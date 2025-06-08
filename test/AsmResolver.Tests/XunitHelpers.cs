using System.Runtime.InteropServices;
using AsmResolver.PE.File;
using Xunit;

namespace AsmResolver.Tests;

public static class XunitHelpers
{
    public static bool IsRunningOnCompatible(MachineType machine) => machine switch
    {
        MachineType.I386 => RuntimeInformation.ProcessArchitecture is Architecture.X86 or Architecture.X64,
        MachineType.Amd64 => RuntimeInformation.ProcessArchitecture is Architecture.X64,
        MachineType.Arm64 => RuntimeInformation.ProcessArchitecture is Architecture.Arm64,
        _ => false
    };

    public static void SkipIfNotMachine(MachineType machine)
    {
        bool compatible = IsRunningOnCompatible(machine);

        Skip.IfNot(compatible, $"Test requires a {machine}-compatible architecture to run.");
    }

    public static void SkipIfNotX86OrX64() => SkipIfNotMachine(MachineType.I386);

    public static void SkipIfNotX64() => SkipIfNotMachine(MachineType.Amd64);

    public static void SkipIfNotArm64() => SkipIfNotMachine(MachineType.Arm64);
}
