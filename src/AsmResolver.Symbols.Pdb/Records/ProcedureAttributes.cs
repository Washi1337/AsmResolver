using System;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Provides members defining all possible flags that can be used to describe the nature of a procedure symbol.
/// </summary>
[Flags]
public enum ProcedureAttributes : byte
{
    /// <summary>
    /// Indicates the frame pointer is present.
    /// </summary>
    NoFpo = 1 << 0,

    /// <summary>
    /// Indicates the function uses an interrupt return.
    /// </summary>
    InterruptReturn = 1 << 1,

    /// <summary>
    /// Indicates the function uses a far return.
    /// </summary>
    FarReturn = 1 << 2,

    /// <summary>
    /// Indicates the function does not return.
    /// </summary>
    NeverReturn = 1 << 3,

    /// <summary>
    /// Indicates the function label is not fallen into.
    /// </summary>
    NotReached = 1 << 4,

    /// <summary>
    /// Indicates the function uses a custom calling convention.
    /// </summary>
    CustomCallingConvention = 1 << 5,

    /// <summary>
    /// Indicates the function is marked as <c>noinline</c>.
    /// </summary>
    NoInline = 1 << 6,

    /// <summary>
    /// Indicates the function has debug information for optimized code.
    /// </summary>
    OptimizedDebugInfo = 1 << 7,
}
