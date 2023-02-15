using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="BitFieldTypeRecord"/> that is read from a PDB image.
/// </summary>
public class SerializedBitFieldTypeRecord : BitFieldTypeRecord
{
    private readonly PdbReaderContext _context;
    private readonly uint _baseTypeIndex;

    /// <summary>
    /// Reads a bit field type from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the bit field type is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedBitFieldTypeRecord(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _baseTypeIndex = reader.ReadUInt32();
        Length = reader.ReadByte();
        Position = reader.ReadByte();
    }

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetBaseType()
    {
        return _context.ParentImage.TryGetLeafRecord(_baseTypeIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Bit field type {TypeIndex:X8} contains an invalid underlying base type index {_baseTypeIndex:X8}.");
    }
}
