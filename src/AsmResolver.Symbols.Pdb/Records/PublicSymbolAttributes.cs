using System;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Provides members defining all flags that can be associated to a public symbol.
/// </summary>
[Flags]
public enum PublicSymbolAttributes : uint
{
    /// <summary>
    /// Indicates no flags are assigned to the symbol.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates the symbol is a code symbol.
    /// </summary>
    Code = 0x00000001,

    /// <summary>
    /// Indicates the symbol is a function.
    /// </summary>
    Function = 0x00000002,

    /// <summary>
    /// Indicates the symbol involves managed code.
    /// </summary>
    Managed = 0x00000004,

    /// <summary>
    /// Indicates the symbol involves MSIL code.
    /// </summary>
    Msil = 0x00000008,
}
