namespace AsmResolver.Symbols.Pdb.Metadata.Dbi;

/// <summary>
/// Provides members describing all possible attributes that can be assigned to a single section map entry.
/// </summary>
public enum SectionMapAttributes : ushort
{
    /// <summary>
    /// Indicates the segment is readable.
    /// </summary>
    Read = 1 << 0,

    /// <summary>
    /// Indicates the segment is writable.
    /// </summary>
    Write = 1 << 1,

    /// <summary>
    /// Indicates the segment is executable.
    /// </summary>
    Execute = 1 << 2,

    /// <summary>
    /// Indicates the descriptor describes a 32-bit linear address.
    /// </summary>
    AddressIs32Bit = 1 << 3,

    /// <summary>
    /// Indicates the frame represents a selector.
    /// </summary>
    IsSelector = 1 << 8,

    /// <summary>
    /// Indicates the frame represents an absolute address.
    /// </summary>
    IsAbsoluteAddress = 1 << 9,

    /// <summary>
    /// Indicates the descriptor represents a group.
    /// </summary>
    IsGroup = 1 << 10
}
