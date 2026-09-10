using System;

namespace AsmResolver.PE.Configuration;

/// <summary>
/// Provides members describing all control flow guard flags that can be specified in a load configuration
/// of a PE image.
/// </summary>
[Flags]
public enum GuardFlags : uint
{
    /// <summary>
    /// Module performs control flow integrity checks using system-supplied support.
    /// </summary>
    CfInstrumented = 0x00000100,

    /// <summary>
    /// Module performs control flow and write integrity checks.
    /// </summary>
    CfwInstrumented = 0x00000200,

    /// <summary>
    /// Module contains valid control flow target metadata.
    /// </summary>
    CfFunctionTablePresent = 0x00000400,

    /// <summary>
    /// Module does not make use of the /GS security cookie.
    /// </summary>
    SecurityCookieUnused = 0x00000800,

    /// <summary>
    /// Module supports read only delay load IAT.
    /// </summary>
    ProtectDelayloadIat = 0x00001000,

    /// <summary>
    /// Delayload import table in its own .didat section (with nothing else in it) that can be freely reprotected.
    /// </summary>
    DelayloadIatInItsOwnSection = 0x00002000,

    /// <summary>
    /// Module contains suppressed export information. This also infers that the address taken IAT table is also present in the load config.
    /// </summary>
    CfExportSuppressionInfoPresent = 0x00004000,

    /// <summary>
    /// Module enables suppression of exports.
    /// </summary>
    CfEnableExportSuppression = 0x00008000,

    /// <summary>
    /// Module contains longjmp target information.
    /// </summary>
    CfLongJumpTablePresent = 0x00010000,

    /// <summary>
    /// Mask for the subfield that contains the stride of Control Flow Guard function table entries
    /// (that is, the additional count of bytes per table entry).
    /// </summary>
    CfFunctionTableSizeMask = 0xF0000000,
}
