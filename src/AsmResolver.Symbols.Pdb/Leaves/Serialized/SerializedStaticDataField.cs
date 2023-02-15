using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="StaticDataField"/> that is read from a PDB image.
/// </summary>
public class SerializedStaticDataField : StaticDataField
{
    private readonly PdbReaderContext _context;
    private readonly uint _dataTypeIndex;
    private readonly BinaryStreamReader _nameReader;

    /// <summary>
    /// Reads a static data member list from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the member is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the member.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedStaticDataField(PdbReaderContext context, uint typeIndex, ref BinaryStreamReader reader)
        : base(typeIndex)
    {
        Attributes = (CodeViewFieldAttributes) reader.ReadUInt16();
        _dataTypeIndex = reader.ReadUInt32();

        _context = context;
        _nameReader = reader;
        reader.AdvanceUntil(0, true);
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetDataType()
    {
        if (_dataTypeIndex == 0)
            return null;

        return _context.ParentImage.TryGetLeafRecord(_dataTypeIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Static data member {TypeIndex:X8} contains an invalid data type index {_dataTypeIndex:X8}.");
    }
}
