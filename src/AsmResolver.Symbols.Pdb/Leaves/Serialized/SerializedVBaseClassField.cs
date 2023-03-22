using System;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="VBaseClassField"/> that is read from a PDB image.
/// </summary>
public class SerializedVBaseClassField : VBaseClassField
{
    private readonly PdbReaderContext _context;
    private readonly uint _baseTypeIndex;
    private readonly uint _basePointerIndex;

    /// <summary>
    /// Reads a virtual base class from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the class is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    /// <param name="reader">The input stream to read from.</param>
    /// <param name="isIndirect"><c>true</c> if the field is an indirect virtual base class, <c>false</c> otherwise.</param>
    public SerializedVBaseClassField(
        PdbReaderContext context,
        uint typeIndex,
        ref BinaryStreamReader reader,
        bool isIndirect)
        : base(typeIndex)
    {
        _context = context;
        Attributes = (CodeViewFieldAttributes) reader.ReadUInt16();
        _baseTypeIndex = reader.ReadUInt32();
        _basePointerIndex = reader.ReadUInt32();
        PointerOffset = Convert.ToUInt64(ReadNumeric(ref reader));
        TableOffset = Convert.ToUInt64(ReadNumeric(ref reader));
        IsIndirect = isIndirect;
    }

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetBaseType()
    {
        return _context.ParentImage.TryGetLeafRecord(_baseTypeIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Virtual base class {TypeIndex:X8} contains an invalid base type index {_baseTypeIndex:X8}.");
    }

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetBasePointerType()
    {
        return _context.ParentImage.TryGetLeafRecord(_basePointerIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Virtual base class {TypeIndex:X8} contains an invalid base pointer type index {_basePointerIndex:X8}.");
    }
}
