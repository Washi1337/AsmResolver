using System;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="BaseClassField"/> that is read from a PDB image.
/// </summary>
public class SerializedBaseClassField : BaseClassField
{
    private readonly PdbReaderContext _context;
    private readonly uint _baseTypeIndex;

    /// <summary>
    /// Reads a base class from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the class is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedBaseClassField(PdbReaderContext context, uint typeIndex, ref BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        Attributes = (CodeViewFieldAttributes) reader.ReadUInt16();
        _baseTypeIndex = reader.ReadUInt32();
        Offset = Convert.ToUInt64(ReadNumeric(ref reader));
    }

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetBaseType()
    {
        return _context.ParentImage.TryGetLeafRecord(_baseTypeIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Base class {TypeIndex:X8} contains an invalid underlying base type index {_baseTypeIndex:X8}.");
    }
}
