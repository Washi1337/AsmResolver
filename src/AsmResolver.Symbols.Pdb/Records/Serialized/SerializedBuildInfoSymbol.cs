using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="BuildInfoSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedBuildInfoSymbol : BuildInfoSymbol
{
    private readonly PdbReaderContext _context;
    private readonly uint _idIndex;

    /// <summary>
    /// Reads a build information symbol from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the symbol is situated in.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedBuildInfoSymbol(PdbReaderContext context, BinaryStreamReader reader)
    {
        _context = context;
        _idIndex = reader.ReadUInt32();
    }

    /// <inheritdoc />
    protected override BuildInfoLeaf? GetInfo()
    {
        return _context.ParentImage.TryGetIdLeafRecord(_idIndex, out BuildInfoLeaf? leaf)
            ? leaf
            : _context.Parameters.ErrorListener.BadImageAndReturn<BuildInfoLeaf>(
                $"Build information symbol contains an invalid ID index {_idIndex:X8}.");
    }
}
