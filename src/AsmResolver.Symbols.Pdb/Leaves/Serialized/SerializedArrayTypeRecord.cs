using System;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="ArrayTypeRecord"/> that is read from a PDB image.
/// </summary>
public class SerializedArrayTypeRecord : ArrayTypeRecord
{
    private readonly PdbReaderContext _context;
    private readonly uint _elementTypeIndex;
    private readonly uint _indexTypeIndex;
    private readonly BinaryStreamReader _nameReader;

    /// <summary>
    /// Reads an array type from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the type is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedArrayTypeRecord(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _elementTypeIndex = reader.ReadUInt32();
        _indexTypeIndex = reader.ReadUInt32();
        Length = Convert.ToUInt64(ReadNumeric(ref reader));
        _nameReader = reader.Fork();
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetElementType()
    {
        return _context.ParentImage.TryGetLeafRecord(_elementTypeIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Array type {TypeIndex:X8} contains an invalid element type index {_elementTypeIndex:X8}.");
    }

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetIndexType()
    {
        return _context.ParentImage.TryGetLeafRecord(_indexTypeIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Array type {TypeIndex:X8} contains an invalid index type index {_indexTypeIndex:X8}.");
    }
}
