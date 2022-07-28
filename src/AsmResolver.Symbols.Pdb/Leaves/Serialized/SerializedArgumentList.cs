using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="ArgumentList"/> that is read from a PDB image.
/// </summary>
public class SerializedArgumentList : ArgumentList
{
    private readonly PdbReaderContext _context;
    private readonly BinaryStreamReader _reader;

    /// <summary>
    /// Reads a argument list from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the list is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the list.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedArgumentList(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _reader = reader;
    }

    /// <inheritdoc />
    protected override IList<CodeViewType> GetArgumentTypes()
    {
        var reader = _reader.Fork();

        int count = reader.ReadInt32();
        var result = new List<CodeViewType>(count);

        for (int i = 0; i < count; i++)
        {
            uint typeIndex = reader.ReadUInt32();
            if (!_context.ParentImage.TryGetLeafRecord(typeIndex, out var leaf) || leaf is not CodeViewType t)
            {
                _context.Parameters.ErrorListener.BadImage(
                    $"Argument list {TypeIndex:X8} contains an invalid argument type index {typeIndex:X8}.");
                return result;
            }

            result.Add(t);
        }

        return result;
    }
}
