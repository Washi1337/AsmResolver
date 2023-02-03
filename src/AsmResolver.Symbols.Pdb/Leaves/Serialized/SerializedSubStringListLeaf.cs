using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="SubStringListLeaf"/> that is read from a PDB image.
/// </summary>
public class SerializedSubStringListLeaf : SubStringListLeaf
{
    private readonly PdbReaderContext _context;
    private readonly BinaryStreamReader _reader;

    /// <summary>
    /// Reads a list of sub-strings from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the member is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the member.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedSubStringListLeaf(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _reader = reader;
    }

    /// <inheritdoc />
    protected override IList<StringIdLeaf> GetEntries()
    {
        var reader = _reader.Fork();
        uint count = reader.ReadUInt32();
        return ReadEntries(_context, TypeIndex, count, ref reader);
    }

    internal static IList<StringIdLeaf> ReadEntries(
        PdbReaderContext context,
        uint originIndex,
        uint count,
        ref BinaryStreamReader reader)
    {
        var result = new List<StringIdLeaf>();

        for (int i = 0; i < count; i++)
        {
            uint index = reader.ReadUInt32();
            if (!context.ParentImage.TryGetIdLeafRecord(index, out StringIdLeaf? entry))
            {
                context.Parameters.ErrorListener.BadImage(
                    $"String List {originIndex:X8} contains an invalid string index {index:X8}.");
                return result;
            }
            result.Add(entry);
        }

        return result;
    }

}
