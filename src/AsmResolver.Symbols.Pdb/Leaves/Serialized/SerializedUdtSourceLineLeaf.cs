using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="UdtSourceLineLeaf"/> that is read from a PDB image.
/// </summary>
public class SerializedUdtSourceLineLeaf : UdtSourceLineLeaf
{
    private readonly PdbReaderContext _context;
    private readonly uint _udtTypeIndex;
    private readonly uint _sourceFileIndex;

    /// <summary>
    /// Reads a UDT source line from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the leaf is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the leaf.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedUdtSourceLineLeaf(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;

        _udtTypeIndex = reader.ReadUInt32();
        _sourceFileIndex = reader.ReadUInt32();
        Line = reader.ReadUInt32();
    }

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetUdtType()
    {
        return _context.ParentImage.TryGetLeafRecord(_udtTypeIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"UDT source line {TypeIndex:X8} contains an invalid UDT type index {_udtTypeIndex:X8}.");
    }

    /// <inheritdoc />
    protected override ICodeViewLeaf? GetSourceFile()
    {
        return _context.ParentImage.TryGetIdLeafRecord(_sourceFileIndex, out var leaf)
            ? leaf
            : _context.Parameters.ErrorListener.BadImageAndReturn<ICodeViewLeaf>(
                $"UDT source line {TypeIndex:X8} contains an invalid source file index {_sourceFileIndex:X8}.");
    }
}
