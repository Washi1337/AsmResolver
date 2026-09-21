namespace AsmResolver.PE.Configuration;

/// <summary>
/// Provides known sizes for load configuration data directories in a PE image.
/// </summary>
public static class LoadConfigurationSizes
{
    /// <summary>32-bit (x86) Structure Sizes</summary>
    public static class X86
    {
        /// <summary>Windows XP / Server 2003 (Base structure)</summary>
        public const uint WinXP = 0x40;

        /// <summary>Windows Vista / Server 2008 (SafeSEH, SecurityCookie)</summary>
        public const uint WinVista = 0x48;

        /// <summary>Windows 8.1 / 10 1507 (Control Flow Guard - CFG)</summary>
        public const uint Win81 = 0x5C;

        /// <summary>Windows 10 1607 (CFG Additions, CodeIntegrity, LongJump)</summary>
        public const uint Win10_1607 = 0x78;

        /// <summary>Windows 10 1703 / 1709 (Return Flow Guard - RFG, CHPE)</summary>
        public const uint Win10_1703 = 0x8C;

        /// <summary>Windows 10 1803 / 1809 (Enclaves, VolatileMetadata)</summary>
        public const uint Win10_1803 = 0xA4;

        /// <summary>Windows 10 2004 / 20H2 (Extended Flow Guard - XFG, EHContinuation)</summary>
        public const uint Win10_2004 = 0xB8;

        /// <summary>Windows 11 21H2+ (CastGuard, Memcpy, UMA)</summary>
        public const uint Win11_21H2 = 0xC8;
    }

    /// <summary>64-bit (x64) Structure Sizes</summary>
    public static class X64
    {
        /// <summary>Windows XP / Server 2003 (Base structure)</summary>
        public const uint WinXP = 0x48;

        /// <summary>Windows Vista / Server 2008 (SafeSEH, SecurityCookie)</summary>
        public const uint WinVista = 0x70;

        /// <summary>Windows 8.1 / 10 1507 (Control Flow Guard - CFG)</summary>
        public const uint Win81 = 0x94;

        /// <summary>Windows 10 1607 (CFG Additions, CodeIntegrity, LongJump)</summary>
        public const uint Win10_1607 = 0xC0;

        /// <summary>Windows 10 1703 / 1709 (Return Flow Guard - RFG, CHPE)</summary>
        public const uint Win10_1703 = 0xE0;

        /// <summary>Windows 10 1803 / 1809 (Enclaves, VolatileMetadata)</summary>
        public const uint Win10_1803 = 0x100;

        /// <summary>Windows 10 2004 / 20H2 (Extended Flow Guard - XFG, EHContinuation)</summary>
        public const uint Win10_2004 = 0x130;

        /// <summary>Windows 11 21H2+ (CastGuard, Memcpy, UMA)</summary>
        public const uint Win11_21H2 = 0x148;
    }
}
