using System;

namespace AsmResolver.PE.Configuration;

/// <summary>
/// Provides members describing all possible flags that can be associated to a control flow guard function.
/// </summary>
[Flags]
public enum ControlFlowGuardFunctionFlags : byte
{
    /// <summary>
    /// Indicates the call target is explicitly suppressed (do not treat it as valid for purposes of CFG).
    /// </summary>
    FidSuppressed = 1,

    /// <summary>
    /// Indicates the call target is export suppressed.
    /// </summary>
    ExportSuppressed = 2
}
`
