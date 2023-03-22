using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="BuildInfoLeaf"/> that is read from a PDB image.
/// </summary>
public class SerializedBuildInfoLeaf : BuildInfoLeaf
{
    private readonly PdbReaderContext _context;
    private readonly BinaryStreamReader _reader;

    /// <summary>
    /// Reads a build information leaf from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the member is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the member.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedBuildInfoLeaf(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _reader = reader;
    }

    /// <inheritdoc />
    protected override IList<StringIdentifier> GetEntries()
    {
        var reader = _reader.Fork();
        uint count = reader.ReadUInt16();
        return SerializedSubStringListLeaf.ReadEntries(_context, TypeIndex, count, ref reader);
    }
}
