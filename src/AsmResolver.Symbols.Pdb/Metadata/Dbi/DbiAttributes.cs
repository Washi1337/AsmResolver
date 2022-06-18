using System;

namespace AsmResolver.Symbols.Pdb.Metadata.Dbi;

/// <summary>
/// Provides members defining all attributes that can be assigned to a single DBI stream.
/// </summary>
[Flags]
public enum DbiAttributes : ushort
{
    /// <summary>
    /// Indicates no attributes were assigned.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates the program was linked in an incremental manner.
    /// </summary>
    IncrementallyLinked = 1,

    /// <summary>
    /// Indicates private symbols were stripped from the PDB file.
    /// </summary>
    PrivateSymbolsStripped = 2,

    /// <summary>
    /// Indicates the program was linked using link.exe with the undocumented <c>/DEBUG:CTYPES</c> flag.
    /// </summary>
    HasConflictingTypes = 4,
}
