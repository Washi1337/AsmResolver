using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="UdtModuleSourceLineLeaf"/> that is read from a PDB image.
/// </summary>
public class SerializedUdtModuleSourceLineLeaf : UdtModuleSourceLineLeaf
{
    private readonly PdbReaderContext _context;
    private readonly uint _udtTypeIndex;

    /// <summary>
    /// Reads a UDT module source line from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the leaf is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the leaf.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedUdtModuleSourceLineLeaf(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;

        _udtTypeIndex = reader.ReadUInt32();
        SourceFileOffset = reader.ReadUInt32();
        Line = reader.ReadUInt32();
        Module = reader.ReadUInt16();
    }

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetUdtType()
    {
        return _context.ParentImage.TryGetLeafRecord(_udtTypeIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"UDT module source line {TypeIndex:X8} contains an invalid UDT type index {_udtTypeIndex:X8}.");
    }
}
