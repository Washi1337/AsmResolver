using System;

namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides members defining all possible modifiers that can be added to a type using a Modifier type record in a
/// TPI or IPI stream.
/// </summary>
[Flags]
public enum ModifierAttributes : ushort
{
    /// <summary>
    /// Indicates the type is marked as const.
    /// </summary>
    Const = 1,

    /// <summary>
    /// Indicates the type is marked as volatile.
    /// </summary>
    Volatile = 2,

    /// <summary>
    /// Indicates the type is marked as unaligned.
    /// </summary>
    Unaligned = 4,
}
