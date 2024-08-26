using System;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Describes a C13 lines section.
/// </summary>
[Flags]
public enum C13LinesAttributes : ushort
{
    /// <summary>
    /// Indicates the C13 lines section has column information.
    /// </summary>
    HasColumns = 0x0001
}
