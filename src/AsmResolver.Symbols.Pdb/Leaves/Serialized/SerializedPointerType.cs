using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="PointerType"/> that is read from a PDB image.
/// </summary>
public class SerializedPointerType : PointerType
{
    private readonly PdbReaderContext _context;
    private readonly uint _baseTypeIndex;

    /// <summary>
    /// Reads a pointer type from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the type is situated in.</param>
    /// <param name="typeIndex">The index to assign to the type.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedPointerType(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _baseTypeIndex = reader.ReadUInt32();
        Attributes = (PointerAttributes) reader.ReadUInt32();

        // TODO: member pointer info
    }

    /// <inheritdoc />
    protected override CodeViewType? GetBaseType()
    {
        return _context.ParentImage.TryGetLeafRecord(_baseTypeIndex, out var leaf) && leaf is CodeViewType type
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewType>(
                $"Pointer {TypeIndex:X8} contains an invalid underlying base type index {_baseTypeIndex:X8}.");

    }
}
