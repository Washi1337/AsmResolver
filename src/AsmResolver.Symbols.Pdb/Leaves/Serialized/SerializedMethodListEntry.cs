using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="MethodListEntry"/> that is read from a PDB image.
/// </summary>
public class SerializedMethodListEntry : MethodListEntry
{
    private readonly PdbReaderContext _context;
    private readonly uint _functionIndex;

    /// <summary>
    /// Reads a member function from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the type is situated in.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedMethodListEntry(PdbReaderContext context, ref BinaryStreamReader reader)
    {
        _context = context;
        Attributes = (CodeViewFieldAttributes) reader.ReadUInt16();
        reader.ReadUInt16(); // padding
        _functionIndex = reader.ReadUInt32();
        VTableOffset = IsIntroducingVirtual ? reader.ReadUInt32() : 0;
    }

    /// <inheritdoc />
    protected override MemberFunctionLeaf? GetFunction()
    {
        return _context.ParentImage.TryGetLeafRecord(_functionIndex, out var leaf) && leaf is MemberFunctionLeaf type
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<MemberFunctionLeaf>(
                $"Method list entry contains an invalid function type index {_functionIndex:X8}.");
    }
}
