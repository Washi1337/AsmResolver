using System;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="BaseClass"/> that is read from a PDB image.
/// </summary>
public class SerializedBaseClass : BaseClass
{
    private readonly PdbReaderContext _context;
    private readonly uint _baseTypeIndex;

    /// <summary>
    /// Reads a base class from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the class is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedBaseClass(PdbReaderContext context, uint typeIndex, ref BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        Attributes = (CodeViewFieldAttributes) reader.ReadUInt16();
        _baseTypeIndex = reader.ReadUInt32();
        Offset = Convert.ToUInt64(ReadNumeric(ref reader));
    }

    /// <inheritdoc />
    protected override CodeViewType? GetBaseType()
    {
        return _context.ParentImage.TryGetLeafRecord(_baseTypeIndex, out var leaf) && leaf is CodeViewType type
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewType>(
                $"Base class {TypeIndex:X8} contains an invalid underlying base type index {_baseTypeIndex:X8}.");
    }
}
