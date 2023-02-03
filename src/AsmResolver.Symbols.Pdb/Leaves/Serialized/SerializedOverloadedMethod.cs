using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="OverloadedMethod"/> that is read from a PDB image.
/// </summary>
public class SerializedOverloadedMethod : OverloadedMethod
{
    private readonly PdbReaderContext _context;
    private readonly ushort _functionCount;
    private readonly uint _methodListIndex;
    private readonly BinaryStreamReader _nameReader;

    /// <summary>
    /// Reads an overloaded method from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the method is situated in.</param>
    /// <param name="typeIndex">The index to assign to the type.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedOverloadedMethod(PdbReaderContext context, uint typeIndex, ref BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _functionCount = reader.ReadUInt16();
        _methodListIndex = reader.ReadUInt32();
        _nameReader = reader.Fork();
        reader.AdvanceUntil(0, true);
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override MethodListLeaf? GetMethods()
    {
        if (_methodListIndex == 0)
            return null;

        return _context.ParentImage.TryGetLeafRecord(_methodListIndex, out MethodListLeaf? list)
            ? list
            : _context.Parameters.ErrorListener.BadImageAndReturn<MethodListLeaf>(
                $"Overloaded method {TypeIndex:X8} contains an invalid field list index {_methodListIndex:X8}.");
    }
}
