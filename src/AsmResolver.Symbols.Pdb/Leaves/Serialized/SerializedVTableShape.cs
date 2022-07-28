using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="VTableShape"/> that is read from a PDB image.
/// </summary>
public class SerializedVTableShape : VTableShape
{
    private readonly ushort _count;
    private readonly BinaryStreamReader _entriesReader;

    /// <summary>
    /// Reads a virtual function table shape from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the shape is situated in.</param>
    /// <param name="typeIndex">The index to assign to the shape.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedVTableShape(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _count = reader.ReadUInt16();
        _entriesReader = reader;
    }

    /// <inheritdoc />
    protected override IList<VTableShapeEntry> GetEntries()
    {
        var result = new List<VTableShapeEntry>(_count);
        var reader = _entriesReader.Fork();

        // Entries are stored as 4-bit values.
        byte currentByte = 0;
        for (int i = 0; i < _count; i++)
        {
            if (i % 2 == 0)
            {
                currentByte = reader.ReadByte();
                result.Add((VTableShapeEntry) (currentByte & 0xF));
            }
            else
            {
                result.Add((VTableShapeEntry) ((currentByte >> 4) & 0xF));
            }
        }

        return result;
    }
}
