using System;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="InstanceDataField"/> that is read from a PDB image.
/// </summary>
public class SerializedInstanceDataField : InstanceDataField
{
    private readonly PdbReaderContext _context;
    private readonly uint _dataTypeIndex;
    private readonly BinaryStreamReader _nameReader;

    /// <summary>
    /// Reads an instance data member list from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the member is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the member.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedInstanceDataField(PdbReaderContext context, uint typeIndex, ref BinaryStreamReader reader)
        : base(typeIndex)
    {
        Attributes = (CodeViewFieldAttributes) reader.ReadUInt16();
        _dataTypeIndex = reader.ReadUInt32();

        // We need to eagerly initialize the offset because it is the only way to know how large the leaf is.
        Offset = Convert.ToUInt64(ReadNumeric(ref reader));

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
                $"Instance data member {TypeIndex:X8} contains an invalid data type index {_dataTypeIndex:X8}.");
    }
}
