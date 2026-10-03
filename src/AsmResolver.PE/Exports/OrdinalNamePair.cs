using System;

namespace AsmResolver.PE.Exports;

/// <summary>
/// Represents a single entry in the ordinal-name table of an export directory.
/// </summary>
/// <param name="RelativeOrdinal">The ordinal, relative to the base ordinal.</param>
/// <param name="Name">The name of the export associated to the ordinal.</param>
public readonly record struct OrdinalNamePair(ushort RelativeOrdinal, string Name) : IComparable<OrdinalNamePair>
{
    /// <inheritdoc />
    public int CompareTo(OrdinalNamePair other) => string.Compare(Name, other.Name, StringComparison.Ordinal);
}
