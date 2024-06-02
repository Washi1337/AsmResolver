using System;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides members defining all possible flags that can be assigned to a P/Invoke implementation mapping.
    /// </summary>
    [Flags]
    public enum ImplementationMapAttributes : ushort
    {
        /// <summary>
        /// Indicates no name mangling was used.
        /// </summary>
        NoMangle = 0x0001,

        /// <summary>
        /// Indicates the character set was not specified.
        /// </summary>
        CharSetNotSpec = 0x0000,

        /// <summary>
        /// Indicates the character set used is ANSI.
        /// </summary>
        CharSetAnsi = 0x0002,

        /// <summary>
        /// Indicates the character set used is unicode.
        /// </summary>
        CharSetUnicode = 0x0004,

        /// <summary>
        /// Indicates the character set is determined by the runtime.
        /// </summary>
        CharSetAuto = 0x0006,

        /// <summary>
        /// Provides a mask for the character set.
        /// </summary>
        CharSetMask = 0x0006,

        /// <summary>
        /// Indicates best fit mapping behavior when converting Unicode characters to ANSI characters is determined
        /// by the runtime.
        /// </summary>
        BestFitUseAssem = 0x0000,

        /// <summary>
        /// Indicates best-fit mapping behavior when converting Unicode characters to ANSI characters is enabled.
        /// </summary>
        BestFitEnabled = 0x0010,

        /// <summary>
        /// Indicates best-fit mapping behavior when converting Unicode characters to ANSI characters is disabled.
        /// </summary>
        BestFitDisabled = 0x0020,

        /// <summary>
        /// Provides a mask for the best-fit behaviour.
        /// </summary>
        BestFitMask = 0x0030,

        /// <summary>
        /// Indicates the throw behaviour on an unmappable Unicode character is undefined.
        /// </summary>
        ThrowOnUnmappableCharUseAssem = 0x0000,

        /// <summary>
        /// Indicates the runtime will throw an exception on an unmappable Unicode character that is converted to an
        /// ANSI "?" character.
        /// </summary>
        ThrowOnUnmappableCharEnabled = 0x1000,

        /// <summary>
        /// Indicates the runtime will not throw an exception on an unmappable Unicode character that is converted to an
        /// ANSI "?" character.
        /// </summary>
        ThrowOnUnmappableCharDisabled = 0x2000,

        /// <summary>
        /// Provides a mask for the throw on unmappable behaviour.
        /// </summary>
        ThrowOnUnmappableCharMask = 0x3000,

        /// <summary>
        /// Indicates whether the callee calls the SetLastError Win32 API function before returning from the attributed
        /// method.
        /// </summary>
        SupportsLastError = 0x0040,

        /// <summary>
        /// Indicates P/Invoke will use the native calling convention appropriate to target windows platform.
        /// </summary>
        CallConvWinapi = 0x0100,

        /// <summary>
        /// Indicates P/Invoke will use the C calling convention.
        /// </summary>
        CallConvCdecl = 0x0200,

        /// <summary>
        /// Indicates P/Invoke will use the stdcall calling convention.
        /// </summary>
        CallConvStdcall = 0x0300,

        /// <summary>
        /// Indicates P/Invoke will use the thiscall calling convention.
        /// </summary>
        CallConvThiscall = 0x0400,

        /// <summary>
        /// Indicates P/Invoke will use the fastcall calling convention.
        /// </summary>
        CallConvFastcall = 0x0500,

        /// <summary>
        /// Provides a mask for the calling convention flags.
        /// </summary>
        CallConvMask = 0x0700,
    }
}
