namespace AsmResolver.PE.File
{
    /// <summary>
    /// Provides members for each processor architecture that a portable executable (PE) file can encode for.
    /// </summary>
    public enum MachineType : ushort
    {
        /// <summary>
        /// The content of this field is assumed to be applicable to any machine type.
        /// </summary>
        Unknown = 0x0000,

        /// <summary>
        /// Provides a bitmask that is used to indicate the PE file is targeting Apple devices only.
        /// </summary>
        /// <remarks>
        /// This value is a bitmask and is not a valid machine type on its own. Furthermore, is only supposed to be
        /// used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        DotNetAppleOSOverride = 0x4644,

        /// <summary>
        /// Provides a bitmask that is used to indicate the PE file is targeting FreeBSD operating system derivatives.
        /// </summary>
        /// <remarks>
        /// This value is a bitmask and is not a valid machine type on its own. Furthermore, is only supposed to be
        /// used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        DotNetFreeBsdOSOverride = 0xADC4,


        /// <summary>
        /// Provides a bitmask that is used to indicate the PE file is targeting Linux operating system derivatives.
        /// </summary>
        /// <remarks>
        /// This value is a bitmask and is not a valid machine type on its own. Furthermore, is only supposed to be
        /// used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        DotNetLinuxOSOverride = 0x7B79,

        /// <summary>
        /// Provides a bitmask that is used to indicate the PE file is targeting NetBSD operating system derivatives.
        /// </summary>
        /// <remarks>
        /// This value is a bitmask and is not a valid machine type on its own. Furthermore, is only supposed to be
        /// used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        DotNetNetBsdOSOverride = 0x1993,

        /// <summary>
        /// Provides a bitmask that is used to indicate the PE file is targeting Sun operating system derivatives.
        /// </summary>
        /// <remarks>
        /// This value is a bitmask and is not a valid machine type on its own. Furthermore, is only supposed to be
        /// used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        DotNetSunOSOverride = 0x1992,

        /// <summary>
        /// Indicates the Matsushita AM33 architecture.
        /// </summary>
        Am33 = 0x01D3,

        /// <summary>
        /// Indicates the x64 architecture.
        /// </summary>
        Amd64 = 0x8664,

        /// <summary>
        /// Indicates the x64 architecture, but specifically targets Apple devices.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        Amd64DotNetApple = Amd64 ^ DotNetAppleOSOverride,

        /// <summary>
        /// Indicates the x64 architecture, but specifically targets FreeBSD operating system derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        Amd64DotNetFreeBsd = Amd64 ^ DotNetFreeBsdOSOverride,

        /// <summary>
        /// Indicates the x64 architecture, but specifically targets Linux operating system derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        Amd64DotNetLinux = Amd64 ^ DotNetLinuxOSOverride,

        /// <summary>
        /// Indicates the x64 architecture, but specifically targets NetBSD operating system derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        Amd64DotNetNetBsd = Amd64 ^ DotNetNetBsdOSOverride,

        /// <summary>
        /// Indicates the x64 architecture, but specifically targets SunOS operating system derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        Amd64DotNetSun = Amd64 ^ DotNetSunOSOverride,

        /// <summary>
        /// Indicates the ARM little endian architecture.
        /// </summary>
        Arm = 0x01C0,

        /// <summary>
        /// Indicates the ARM thumb-2 little endian architecture.
        /// </summary>
        ArmNt = 0x01C4,

        /// <summary>
        /// Indicates the ARM thumb-2 little endian architecture, but specifically targets Apple devices.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        ArmNtDotNetApple = ArmNt ^ DotNetAppleOSOverride,

        /// <summary>
        /// Indicates the ARM thumb-2 little endian architecture, but specifically targets FreeBSD operating system
        /// derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        ArmNtDotNetFreeBsd = ArmNt ^ DotNetFreeBsdOSOverride,

        /// <summary>
        /// Indicates the ARM thumb-2 little endian architecture, but specifically targets Linux operating system
        /// derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        ArmNtDotNetLinux = ArmNt ^ DotNetLinuxOSOverride,

        /// <summary>
        /// Indicates the ARM thumb-2 little endian architecture, but specifically targets SunOS operating system
        /// derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        ArmNtDotNetSun = ArmNt ^ DotNetSunOSOverride,

        /// <summary>
        /// Indicates the ARM thumb-2 little endian architecture, but specifically targets NetBSD operating system
        /// derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        ArmNtDotNetNetBsd = ArmNt ^ DotNetNetBsdOSOverride,

        /// <summary>
        /// Indicates the ARM 64-bit little endian architecture.
        /// </summary>
        Arm64 = 0xAA64,

        /// <summary>
        /// Indicates the ARM 64-bit little endian architecture, but specifically targets Apple devices.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        Arm64DotNetApple = Arm64 ^ DotNetAppleOSOverride,

        /// <summary>
        /// Indicates the ARM 64-bit little endian architecture, but specifically targets FreeBSD operating system
        /// derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        Arm64DotNetFreeBsd = Arm64 ^ DotNetFreeBsdOSOverride,

        /// <summary>
        /// Indicates the ARM 64-bit little endian architecture, but specifically targets Linux operating system
        /// derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        Arm64DotNetLinux = Arm64 ^ DotNetLinuxOSOverride,

        /// <summary>
        /// Indicates the ARM 64-bit little endian architecture, but specifically targets NetBSD operating system
        /// derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        Arm64DotNetNetBsd = Arm64 ^ DotNetNetBsdOSOverride,

        /// <summary>
        /// Indicates the ARM 64-bit little endian architecturee, but specifically targets SunOS operating system derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        Arm64DotNetSun = Arm64 ^ DotNetSunOSOverride,

        /// <summary>
        /// Indicates EFI byte code.
        /// </summary>
        Ebc = 0x0EBC,

        /// <summary>
        /// Indicates the Intel 368 or compatible x86 (32-bit) architectures.
        /// </summary>
        I386 = 0x014C,

        /// <summary>
        /// Indicates the Intel 368 or compatible x86 (32-bit) architectures, but specifically targets
        /// Apple devices.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        I386DotNetApple = I386 ^ DotNetAppleOSOverride,

        /// <summary>
        /// Indicates the Intel 368 or compatible x86 (32-bit) architectures, but specifically targets FreeBSD operating
        /// system derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        I386DotNetFreeBsd = I386 ^ DotNetFreeBsdOSOverride,

        /// <summary>
        /// Indicates the Intel 368 or compatible x86 (32-bit) architectures, but specifically targets Linux
        /// operating system derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        I386DotNetLinux = I386 ^ DotNetLinuxOSOverride,

        /// <summary>
        /// Indicates the Intel 368 or compatible x86 (32-bit) architectures, but specifically targets NetBSD
        /// operating system derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        I386DotNetNetBsd = I386 ^ DotNetNetBsdOSOverride,

        /// <summary>
        /// Indicates the Intel 368 or compatible x86 (32-bit) architectures, but specifically targets SunOS
        /// operating system derivatives.
        /// </summary>
        /// <remarks>
        /// This value is only used by .NET PE images.
        /// Reference: https://github.com/dotnet/runtime/issues/36364
        /// </remarks>
        I386DotNetSun = I386 ^ DotNetSunOSOverride,

        /// <summary>
        /// Indicates the Intel Itanium processor family.
        /// </summary>
        Ia64 = 0x0200,

        /// <summary>
        /// Indicates the Mitsubishi M32R little endian architecture.
        /// </summary>
        M32R = 0x9041,

        /// <summary>
        /// Indicates the MIPS 16-bit architecture.
        /// </summary>
        Mips16 = 0x0266,

        /// <summary>
        /// Indicates the MIPS architecture with FPU.
        /// </summary>
        MipsFpu = 0x0366,

        /// <summary>
        /// Indicates the MIPS 16-bit architecture with FPU.
        /// </summary>
        MipsFpu16 = 0x0466,

        /// <summary>
        /// Indicates the PowerPC little-endian architecture.
        /// </summary>
        PowerPc = 0x01F0,

        /// <summary>
        /// Indicates the PowerPC little-endian architecture with floating point support.
        /// </summary>
        PowerPcFp = 0x01F1,

        /// <summary>
        /// Indicates the MIPS little endian architecture.
        /// </summary>
        R4000 = 0x0166,

        /// <summary>
        /// Indicates the Hitachi SH3 architecture.
        /// </summary>
        Sh3 = 0x01A2,

        /// <summary>
        /// Indicates the Hitachi SH3 DSP architecture.
        /// </summary>
        Sh3Dsp = 0x01A3,

        /// <summary>
        /// Indicates the Hitachi SH4 architecture.
        /// </summary>
        Sh4 = 0x01A6,

        /// <summary>
        /// Indicates the Hitachi SH5 architecture.
        /// </summary>
        Sh5 = 0x01A8,

        /// <summary>
        /// Indicates the Thumb architecture.
        /// </summary>
        Thumb = 0x01C2,

        /// <summary>
        /// Indicates the MIPS little-endian WCE v2 architecture.
        /// </summary>
        WceMipsV2 = 0x0169,

        /// <summary>
        /// Indicates the RISCV 32-bit architecture.
        /// </summary>
        RiscV32 = 0x5032,

        /// <summary>
        /// Indicates the RISCV 64-bit architecture.
        /// </summary>
        RiscV64 = 0x5064,

        /// <summary>
        /// Indicates the RISCV 128-bit architecture.
        /// </summary>
        RiscV128 = 0x5128,
    }
}
