using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="MethodList"/> that is read from a PDB image.
/// </summary>
public class SerializedMethodList : MethodList
{
    private readonly PdbReaderContext _context;
    private readonly BinaryStreamReader _reader;

    /// <summary>
    /// Reads a method list from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the list is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the enum type.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedMethodList(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _reader = reader;
    }

    /// <inheritdoc />
    protected override IList<MethodListEntry> GetEntries()
    {
        var result = new List<MethodListEntry>();

        var reader = _reader.Fork();
        while (reader.CanRead(8))
            result.Add(new SerializedMethodListEntry(_context, ref reader));

        return result;
    }
}
