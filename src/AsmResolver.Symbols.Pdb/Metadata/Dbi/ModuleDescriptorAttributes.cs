using System;

namespace AsmResolver.Symbols.Pdb.Metadata.Dbi;

/// <summary>
/// Defines all possible flags that can be assigned to a module descriptor.
/// </summary>
[Flags]
public enum ModuleDescriptorAttributes : ushort
{
    /// <summary>
    /// Indicates the module has been written to since reading the PDB.
    /// </summary>
    Dirty = 1,

    /// <summary>
    /// Indicates the module contains Edit &amp; Continue information.
    /// </summary>
    EC = 2,

    /// <summary>
    /// Provides a mask for the type server index that is stored within the flags.
    /// </summary>
    TsmMask = 0xFF00,
}
