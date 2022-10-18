using System.Collections.Generic;

namespace AsmResolver.Symbols.Pdb.Metadata.Dbi;

/// <summary>
/// Represents a collection of file paths used as input source code for a single module.
/// </summary>
public class SourceFileCollection : List<Utf8String>
{
    /// <summary>
    /// Creates a new empty source file collection.
    /// </summary>
    public SourceFileCollection()
    {
    }

    /// <summary>
    /// Creates a new empty source file collection.
    /// </summary>
    /// <param name="originalModuleIndex">The original module index for which this collection was compiled.</param>
    public SourceFileCollection(uint originalModuleIndex)
    {
        OriginalModuleIndex = originalModuleIndex;
    }

    /// <summary>
    /// Gets the original module index for which this collection was compiled (if available).
    /// </summary>
    /// <remarks>
    /// The exact purpose of this number is unclear, as this number cannot be reliably used as an index within the
    /// DBI stream's module list. Use the index of this list within <see cref="DbiStream.SourceFiles"/> instead.
    /// </remarks>
    public uint OriginalModuleIndex
    {
        get;
    }
}
