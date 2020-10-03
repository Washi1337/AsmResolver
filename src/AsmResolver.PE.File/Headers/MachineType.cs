namespace AsmResolver.PE.File.Headers
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
        /// Indicates the Matsushita AM33 architecture.
        /// </summary>
        Am33 = 0x01D3,
        
        /// <summary>
        /// Indicates the x64 architecture.
        /// </summary>
        Amd64 = 0x8664,
        
        /// <summary>
        /// Indicates the ARM little endian architecture.
        /// </summary>
        Arm = 0x01C0,
        
        /// <summary>
        /// Indicates the ARM thumb-2 little endian architecture.
        /// </summary>
        ArmNt = 0x01C4,
        
        /// <summary>
        /// Indicates the ARM 64-bit little endian architecture.
        /// </summary>
        Arm64 = 0xAA64,
        
        /// <summary>
        /// Indicates EFI byte code.
        /// </summary>
        Ebc = 0x0EBC,
        
        /// <summary>
        /// Indicates the Intel 368 or compatible x86 (32-bit) architectures.
        /// </summary>
        I386 = 0x014C,
        
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