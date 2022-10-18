using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="ModifierTypeRecord"/> that is read from a PDB image.
/// </summary>
public class SerializedModifierTypeRecord : ModifierTypeRecord
{
    private readonly PdbReaderContext _context;
    private readonly uint _baseTypeIndex;

    /// <summary>
    /// Reads a pointer type from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the type is situated in.</param>
    /// <param name="typeIndex">The index to assign to the type.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedModifierTypeRecord(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _baseTypeIndex = reader.ReadUInt32();
        Attributes = (ModifierAttributes) reader.ReadUInt32();
    }

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetBaseType()
    {
        return _context.ParentImage.TryGetLeafRecord(_baseTypeIndex, out var leaf) && leaf is CodeViewTypeRecord type
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Modifier type {TypeIndex:X8} contains an invalid underlying base type index {_baseTypeIndex:X8}.");
    }
}
